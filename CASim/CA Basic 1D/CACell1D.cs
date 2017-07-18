
namespace CASimulator
{
    /// <summary>
    /// A cell with 1D directional values and a state.
    /// </summary>
    public class CACell1D
    {
        public CACell1D right, left;
        public byte state, prevstate;
        public int x;

        //AutoCell constructors.
        public CACell1D()
        {
            right = left = null;
            state = prevstate = 0;
            x = 0;
        }

        public CACell1D(ref CACell1D right, ref CACell1D left)
        {
            this.right = right;
            this.left = left;
            state = prevstate = 0;
            x = 0;
        }
    }
}