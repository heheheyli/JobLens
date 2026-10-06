namespace JobLens.Tests;

public class UnitTest1
{
    [Fact]
    public void PassingTest()
    {
        Assert.Equal(4, 2 + 2);
    }

    [Fact]
    public void DeliberateFailure()
    {
        Assert.True(false, "Deliberate failure to prove the pipeline gate works");
    }
}
