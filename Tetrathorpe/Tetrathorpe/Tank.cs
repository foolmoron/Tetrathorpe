using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetrathorpe
{
    class Tank : Character
    {
        public Tank()
            : base()
        {

        }

        public static void Initialize()
        {

        }

        public override void setCharacterState(CharacterState state)
        {
            base.setCharacterState(state);

            switch (state)
            {
                case CharacterState.Standing: setTexture("diamond-petal");
                    break;
            }
        }
    }
}
