namespace CASimulator
{
    /// <summary>
    /// A cell with 2D directional values and a state.
    /// </summary>
    public class CACell2D
    {
        public static CACell2D empty;
        public CACell2D right, up, left, down;
        public byte state, prevstate;
        public int x, y;

        static CACell2D()
        {
            empty = new CACell2D();
            empty.right = empty;
            empty.left = empty;
            empty.up = empty;
            empty.down = empty;
        }

        //AutoCell constructors.
        public CACell2D()
        {
            right = up = left = down = null;
            state = prevstate = 0;
            x = y = 0;
        }

        public CACell2D(ref CACell2D right, ref CACell2D up,
            ref CACell2D left, ref CACell2D down)
        {
            this.right = right;
            this.up = up;
            this.left = left;
            this.down = down;
            state = prevstate = 0;
            x = y = 0;
        }
    }
}