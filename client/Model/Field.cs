using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPGameClient.Model
{
    public class Field
    {
        private String representation;

        public Field(String representation)
        {
            this.representation = representation;
        }

        public String getRepresentation()
        {
            return representation;
        }
    }
}
