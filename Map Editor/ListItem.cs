namespace Map_Editor
{
   public class ListItem
    {
       public  string Text { get; set; }
       public byte Version { get; set; }
        public  int Value { get; set; }
        public ListItem(string text, int value)
        {
            Text = text;
            Value = value;
        }


        public override string ToString()
        {
            return Text;
        }


    }
}
