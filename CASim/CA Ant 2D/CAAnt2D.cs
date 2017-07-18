using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CASimulator
{
    /// <summary>
    /// Represents an ant in 2D Langston Ant simulations.
    /// </summary>
    public struct CAAnt2D
    {
        public CACell2DAnt position;
        public int direction; //0 right, 1 up, 2 left, 3 down
        public int type; //Type of ant. Usually left at 0.

        public CAAnt2D(CACell2DAnt position, int direction, int type)
        {
            this.position = position;
            this.direction = direction;
            this.type = type;
        }
    }
}
