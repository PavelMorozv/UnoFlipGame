using System;

namespace GameCore.Enums
{
    [Serializable]
    public enum Action
    {
        Number,
        Give,
        ChangeDirection,
        Flip,
        SkipMove,
        SkipMoveAll,
        Wild,
        WildGive,
        WildGiveForNow,
        None
    }
}
