using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameClient.Model
{
    // simple class containing a representation for a tile
    public class Field
    {
        // the image key for this tile
        private String representation;

        // field is created with the image key
        public Field(String representation)
        {
            this.representation = representation;
        }

        // retrieve the image key
        public String GetRepresentation()
        {
            return representation;
        }
    }
}
