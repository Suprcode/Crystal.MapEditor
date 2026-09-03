using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Map_Editor
{
    public enum MapReferenceLayer
    {
        Back,
        Middle,
        Front
    }

    public enum MapReferenceKind
    {
        Tiles,
        SMTiles,
        Objects,
        Unknown
    }

    internal sealed class MapReferenceSummary
    {
        public MapReferenceLayer Layer { get; set; }
        public MapReferenceKind Kind { get; set; }
        public short LibraryIndex { get; set; }
        public string LibraryName { get; set; }
        public string LibraryFile { get; set; }
        public string LibraryStatus { get; set; }
        public int CellCount { get; set; }
        public HashSet<int> ImageIndices { get; } = new HashSet<int>();
        public Point FirstCell { get; set; }

        public string ImageRange
        {
            get
            {
                if (ImageIndices.Count == 0)
                    return "-";

                return ImageIndices.Min() == ImageIndices.Max()
                    ? ImageIndices.Min().ToString()
                    : string.Format("{0} - {1}", ImageIndices.Min(), ImageIndices.Max());
            }
        }
    }

    internal static class MapReferenceInspector
    {
        public static List<MapReferenceSummary> Scan(CellInfo[,] cells)
        {
            var references = new Dictionary<(MapReferenceLayer Layer, short LibraryIndex), MapReferenceSummary>();

            if (cells == null)
                return new List<MapReferenceSummary>();

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    var cell = cells[x, y];
                    if (cell == null)
                        continue;

                    AddReference(references, cell, MapReferenceLayer.Back, x, y);
                    AddReference(references, cell, MapReferenceLayer.Middle, x, y);
                    AddReference(references, cell, MapReferenceLayer.Front, x, y);
                }
            }

            return references.Values
                .OrderBy(reference => reference.Kind)
                .ThenBy(reference => reference.Layer)
                .ThenBy(reference => reference.LibraryIndex)
                .ToList();
        }

        public static bool HasReference(CellInfo cell, MapReferenceLayer layer)
        {
            return GetImageIndex(cell, layer) >= 0;
        }

        public static short GetLibraryIndex(CellInfo cell, MapReferenceLayer layer)
        {
            switch (layer)
            {
                case MapReferenceLayer.Back:
                    return cell.BackIndex;
                case MapReferenceLayer.Middle:
                    return cell.MiddleIndex;
                case MapReferenceLayer.Front:
                    return cell.FrontIndex;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer));
            }
        }

        public static void SetLibraryIndex(CellInfo cell, MapReferenceLayer layer, short libraryIndex)
        {
            switch (layer)
            {
                case MapReferenceLayer.Back:
                    cell.BackIndex = libraryIndex;
                    break;
                case MapReferenceLayer.Middle:
                    cell.MiddleIndex = libraryIndex;
                    break;
                case MapReferenceLayer.Front:
                    cell.FrontIndex = libraryIndex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer));
            }
        }

        public static string GetLibraryName(short libraryIndex)
        {
            if (libraryIndex < 0 || libraryIndex >= Libraries.ListItems.Length)
                return "(invalid index)";

            var listItem = Libraries.ListItems[libraryIndex];
            if (listItem != null && !string.IsNullOrWhiteSpace(listItem.Text))
                return listItem.Text;

            var library = Libraries.MapLibs[libraryIndex];
            if (library != null && IsRealLibraryFileName(library.FileName))
            {
                var fileName = Path.GetFileNameWithoutExtension(library.FileName);
                if (!string.IsNullOrWhiteSpace(fileName))
                    return fileName;
            }

            return "(not loaded)";
        }

        public static string GetLibraryFile(short libraryIndex)
        {
            if (libraryIndex < 0 || libraryIndex >= Libraries.MapLibs.Length)
                return "-";

            var library = Libraries.MapLibs[libraryIndex];
            return library == null || !IsRealLibraryFileName(library.FileName)
                ? "-"
                : library.FileName;
        }

        public static string GetLibraryStatus(short libraryIndex)
        {
            if (libraryIndex < 0 || libraryIndex >= Libraries.MapLibs.Length)
                return "Invalid index";

            var libraryFile = GetLibraryFile(libraryIndex);
            if (libraryFile != "-")
                return File.Exists(libraryFile) ? "Available" : "File missing";

            return Libraries.ListItems[libraryIndex] != null ? "Configured" : "Unknown";
        }

        public static MapReferenceKind GetReferenceKind(short libraryIndex)
        {
            if (libraryIndex < 0 || libraryIndex >= Libraries.MapLibs.Length)
                return MapReferenceKind.Unknown;

            var libraryName = GetLibraryName(libraryIndex).ToLowerInvariant();
            if (libraryName.Contains("smtile"))
                return MapReferenceKind.SMTiles;
            if (libraryName.Contains("tile"))
                return MapReferenceKind.Tiles;
            if (!libraryName.StartsWith("("))
                return MapReferenceKind.Objects;

            if (libraryIndex == 1 || (libraryIndex >= 110 && libraryIndex <= 119))
                return MapReferenceKind.SMTiles;
            if (libraryIndex == 0 || (libraryIndex >= 100 && libraryIndex <= 109) || libraryIndex == 199)
                return MapReferenceKind.Tiles;

            if (libraryIndex >= 200)
            {
                var familyOffset = (libraryIndex - 200) % 100 % 15;
                if (familyOffset == 3)
                    return MapReferenceKind.SMTiles;
                if (familyOffset >= 0 && familyOffset <= 2)
                    return MapReferenceKind.Tiles;
            }

            return MapReferenceKind.Objects;
        }

        private static void AddReference(
            IDictionary<(MapReferenceLayer Layer, short LibraryIndex), MapReferenceSummary> references,
            CellInfo cell,
            MapReferenceLayer layer,
            int x,
            int y)
        {
            var imageIndex = GetImageIndex(cell, layer);
            if (imageIndex < 0)
                return;

            var libraryIndex = GetLibraryIndex(cell, layer);
            var key = (layer, libraryIndex);
            if (!references.TryGetValue(key, out var reference))
            {
                reference = new MapReferenceSummary
                {
                    Layer = layer,
                    Kind = GetReferenceKind(libraryIndex),
                    LibraryIndex = libraryIndex,
                    LibraryName = GetLibraryName(libraryIndex),
                    LibraryFile = GetLibraryFile(libraryIndex),
                    LibraryStatus = GetLibraryStatus(libraryIndex),
                    FirstCell = new Point(x, y)
                };
                references.Add(key, reference);
            }

            reference.CellCount++;
            reference.ImageIndices.Add(imageIndex);
        }

        private static int GetImageIndex(CellInfo cell, MapReferenceLayer layer)
        {
            switch (layer)
            {
                case MapReferenceLayer.Back:
                    return (cell.BackImage & 0x1FFFFFFF) - 1;
                case MapReferenceLayer.Middle:
                    return (cell.MiddleImage & 0x7FFF) - 1;
                case MapReferenceLayer.Front:
                    return (cell.FrontImage & 0x7FFF) - 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer));
            }
        }

        private static bool IsRealLibraryFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) &&
                   !string.Equals(fileName, ".lib", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class MapInformationForm : Form
    {
        private readonly CellInfo[,] _cells;
        private readonly string _mapName;
        private readonly Func<MapReferenceLayer, short, short, int> _remapReference;
        private readonly DataGridView _referencesGrid;
        private readonly Label _summaryLabel;
        private readonly Label _selectedReferenceLabel;
        private readonly Label _targetLibraryLabel;
        private readonly NumericUpDown _targetIndex;
        private readonly ComboBox _categoryFilter;
        private List<MapReferenceSummary> _references = new List<MapReferenceSummary>();

        public MapInformationForm(
            string mapName,
            CellInfo[,] cells,
            Func<MapReferenceLayer, short, short, int> remapReference)
        {
            _mapName = string.IsNullOrWhiteSpace(mapName) ? "Untitled map" : mapName;
            _cells = cells ?? throw new ArgumentNullException(nameof(cells));
            _remapReference = remapReference ?? throw new ArgumentNullException(nameof(remapReference));

            Text = "Map Information - " + _mapName;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 520);
            Size = new Size(1120, 680);
            ShowIcon = false;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var heading = new Label
            {
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Text = _mapName
            };

            var summaryPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6)
            };
            _summaryLabel = new Label { AutoSize = true, Margin = new Padding(0, 6, 24, 0) };
            var filterLabel = new Label { AutoSize = true, Text = "Show:", Margin = new Padding(0, 6, 6, 0) };
            _categoryFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 130
            };
            _categoryFilter.Items.AddRange(new object[] { "All", "Tiles", "SMTiles", "Objects", "Unknown" });
            _categoryFilter.SelectedIndex = 0;
            _categoryFilter.SelectedIndexChanged += (_, _) => PopulateGrid();
            summaryPanel.Controls.Add(_summaryLabel);
            summaryPanel.Controls.Add(filterLabel);
            summaryPanel.Controls.Add(_categoryFilter);

            _referencesGrid = CreateReferencesGrid();
            _referencesGrid.SelectionChanged += ReferencesGrid_SelectionChanged;

            var bottomPanel = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 8, 0, 0)
            };

            var editGroup = new GroupBox
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Text = "Edit selected reference"
            };
            var editPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(8)
            };
            _selectedReferenceLabel = new Label
            {
                AutoSize = true,
                Text = "Select a reference above.",
                Margin = new Padding(0, 7, 20, 0)
            };
            var targetLabel = new Label { AutoSize = true, Text = "New library index:", Margin = new Padding(0, 7, 6, 0) };
            _targetIndex = new NumericUpDown
            {
                Minimum = 0,
                Maximum = Libraries.MapLibs.Length - 1,
                Width = 75
            };
            _targetIndex.ValueChanged += (_, _) => UpdateTargetLibraryLabel();
            _targetLibraryLabel = new Label { AutoSize = true, Margin = new Padding(8, 7, 16, 0) };
            var applyButton = new Button { AutoSize = true, Text = "Apply remap" };
            applyButton.Click += ApplyButton_Click;
            editPanel.Controls.Add(_selectedReferenceLabel);
            editPanel.Controls.Add(targetLabel);
            editPanel.Controls.Add(_targetIndex);
            editPanel.Controls.Add(_targetLibraryLabel);
            editPanel.Controls.Add(applyButton);
            editGroup.Controls.Add(editPanel);

            var actions = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };
            var closeButton = new Button { AutoSize = true, Text = "Close", DialogResult = DialogResult.Cancel };
            var refreshButton = new Button { AutoSize = true, Text = "Refresh" };
            refreshButton.Click += (_, _) => RefreshView();
            actions.Controls.Add(closeButton);
            actions.Controls.Add(refreshButton);

            bottomPanel.Controls.Add(editGroup, 0, 0);
            bottomPanel.Controls.Add(actions, 0, 1);
            root.Controls.Add(heading, 0, 0);
            root.Controls.Add(summaryPanel, 0, 1);
            root.Controls.Add(_referencesGrid, 0, 2);
            root.Controls.Add(bottomPanel, 0, 3);
            Controls.Add(root);
            CancelButton = closeButton;

            RefreshView();
        }

        private static DataGridView CreateReferencesGrid()
        {
            var grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.Fixed3D,
                Dock = DockStyle.Fill,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kind", HeaderText = "Type", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Layer", HeaderText = "Layer", Width = 75 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LibraryIndex", HeaderText = "Library #", Width = 80, ValueType = typeof(short) });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LibraryName", HeaderText = "Library", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LibraryFile", HeaderText = "Library file", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 160 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CellCount", HeaderText = "Cells", Width = 75, ValueType = typeof(int) });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UniqueImages", HeaderText = "Images", Width = 75, ValueType = typeof(int) });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ImageRange", HeaderText = "Image range", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FirstCell", HeaderText = "First cell", Width = 85 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 120 });
            return grid;
        }

        private void RefreshView()
        {
            _references = MapReferenceInspector.Scan(_cells);

            var tileCount = _references.Where(reference => reference.Kind == MapReferenceKind.Tiles).Sum(reference => reference.CellCount);
            var smTileCount = _references.Where(reference => reference.Kind == MapReferenceKind.SMTiles).Sum(reference => reference.CellCount);
            var objectCount = _references.Where(reference => reference.Kind == MapReferenceKind.Objects).Sum(reference => reference.CellCount);
            var unknownCount = _references.Where(reference => reference.Kind == MapReferenceKind.Unknown).Sum(reference => reference.CellCount);
            var totalCells = (long)_cells.GetLength(0) * _cells.GetLength(1);

            _summaryLabel.Text = string.Format(
                "Size: {0} x {1} ({2:N0} cells)   Tile refs: {3:N0}   SMTile refs: {4:N0}   Object refs: {5:N0}   Unknown: {6:N0}   Libraries: {7}",
                _cells.GetLength(0),
                _cells.GetLength(1),
                totalCells,
                tileCount,
                smTileCount,
                objectCount,
                unknownCount,
                _references.Count);

            PopulateGrid();
        }

        private void PopulateGrid()
        {
            if (_referencesGrid == null || _categoryFilter == null)
                return;

            _referencesGrid.Rows.Clear();
            var selectedFilter = _categoryFilter.SelectedItem == null ? "All" : _categoryFilter.SelectedItem.ToString();

            foreach (var reference in _references)
            {
                if (selectedFilter != "All" && reference.Kind.ToString() != selectedFilter)
                    continue;

                var rowIndex = _referencesGrid.Rows.Add(
                    reference.Kind,
                    reference.Layer,
                    reference.LibraryIndex,
                    reference.LibraryName,
                    reference.LibraryFile,
                    reference.CellCount,
                    reference.ImageIndices.Count,
                    reference.ImageRange,
                    string.Format("{0}, {1}", reference.FirstCell.X, reference.FirstCell.Y),
                    reference.LibraryStatus);
                _referencesGrid.Rows[rowIndex].Tag = reference;
            }

            if (_referencesGrid.Rows.Count > 0)
                _referencesGrid.Rows[0].Selected = true;
            else
                _selectedReferenceLabel.Text = "No references match this filter.";
        }

        private void ReferencesGrid_SelectionChanged(object sender, EventArgs e)
        {
            var reference = GetSelectedReference();
            if (reference == null)
                return;

            _selectedReferenceLabel.Text = string.Format(
                "{0} / {1}: #{2} {3} ({4:N0} cells)",
                reference.Kind,
                reference.Layer,
                reference.LibraryIndex,
                reference.LibraryName,
                reference.CellCount);

            _targetIndex.Value = reference.LibraryIndex >= _targetIndex.Minimum && reference.LibraryIndex <= _targetIndex.Maximum
                ? reference.LibraryIndex
                : 0;
            UpdateTargetLibraryLabel();
        }

        private void UpdateTargetLibraryLabel()
        {
            if (_targetLibraryLabel == null || _targetIndex == null)
                return;

            var libraryIndex = (short)_targetIndex.Value;
            _targetLibraryLabel.Text = string.Format(
                "{0} ({1})",
                MapReferenceInspector.GetLibraryName(libraryIndex),
                MapReferenceInspector.GetReferenceKind(libraryIndex));
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            var reference = GetSelectedReference();
            if (reference == null)
            {
                MessageBox.Show(this, "Select a map reference first.", "Map Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var targetLibraryIndex = (short)_targetIndex.Value;
            if (targetLibraryIndex == reference.LibraryIndex)
            {
                MessageBox.Show(this, "Choose a different target library index.", "Map Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmation = MessageBox.Show(
                this,
                string.Format(
                    "Remap {0:N0} {1} reference(s) on the {2} layer from library #{3} to #{4} ({5})?\n\nYou can undo this change from the main editor.",
                    reference.CellCount,
                    reference.Kind,
                    reference.Layer,
                    reference.LibraryIndex,
                    targetLibraryIndex,
                    MapReferenceInspector.GetLibraryName(targetLibraryIndex)),
                "Confirm library remap",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
                return;

            var changedCells = _remapReference(reference.Layer, reference.LibraryIndex, targetLibraryIndex);
            RefreshView();
            MessageBox.Show(
                this,
                string.Format("Updated {0:N0} cell reference(s).", changedCells),
                "Map Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private MapReferenceSummary GetSelectedReference()
        {
            if (_referencesGrid.SelectedRows.Count == 0)
                return null;

            return _referencesGrid.SelectedRows[0].Tag as MapReferenceSummary;
        }
    }
}
