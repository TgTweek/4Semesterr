namespace Game.Application.Movement.Commands
{
    public sealed class FindPathCommand
    {
        public int StartX { get; }
        public int StartY { get; }
        public int TargetX { get; }
        public int TargetY { get; }

        public FindPathCommand(int startX, int startY, int targetX, int targetY)
        {
            StartX = startX;
            StartY = startY;
            TargetX = targetX;
            TargetY = targetY;
        }
    }
}