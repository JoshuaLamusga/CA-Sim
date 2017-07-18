
namespace CASimulator
{
    /// <summary>
    /// A cell with 2D directional values and a state.
    /// </summary>
    public class CACell2DAnt
    {
        public CACell2DAnt right, up, left, down;
        public byte state;
        public int x, y;

        //AutoCell constructors.
        public CACell2DAnt()
        {
            right = up = left = down = null;
            state = 0;
            x = y = 0;
        }

        public CACell2DAnt(ref CACell2DAnt right, ref CACell2DAnt up,
            ref CACell2DAnt left, ref CACell2DAnt down)
        {
            this.right = right;
            this.up = up;
            this.left = left;
            this.down = down;
            state = 0;
            x = y = 0;
        }
    }
}