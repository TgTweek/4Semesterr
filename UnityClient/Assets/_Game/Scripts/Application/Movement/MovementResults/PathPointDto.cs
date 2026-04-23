namespace Game.Application.Movement.Results
{
    public sealed class PathPointDto
    {
        public int X { get; }
        public int Y { get; }

        public PathPointDto(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}