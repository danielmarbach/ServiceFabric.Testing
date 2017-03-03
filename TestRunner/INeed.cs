namespace TestRunner
{
    [Need]
    public interface INeed<TDependency>
    {
        void Need(TDependency dependency);
    }
}