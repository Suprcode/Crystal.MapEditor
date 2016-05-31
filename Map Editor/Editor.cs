using System.Collections.Generic;

namespace Map_Editor
{
    public class Editor
    {
        private Stack<CellInfoData[]> _unDo, _reDo;

        public Editor( )
        {
            _unDo = new Stack<CellInfoData[]>();
            _reDo = new Stack<CellInfoData[]>();
        }

        public CellInfoData[] UnDo
        {
            get
            {
                if (_unDo.Count > 0)
                {
                    return _unDo.Pop();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _unDo.Push((CellInfoData[])value.Clone());
            }
        }

        public CellInfoData[] ReDo
        {
            get
            {
                if (_reDo.Count > 0)
                {
                    return _reDo.Pop();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _reDo.Push((CellInfoData[])value.Clone());
            }
        }

        public int UnDoCount()
        {
            return _unDo.Count;
        }

        public int ReDoCount()
        {
            return _reDo.Count;
        }

        public void UndoClear()
        {
            _unDo.Clear();
        }

        public void ReDoClear()
        {
            _reDo.Clear();
        }
    }

}
