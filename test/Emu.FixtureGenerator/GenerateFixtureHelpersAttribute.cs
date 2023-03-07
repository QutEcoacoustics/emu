namespace Emu.FixtureGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateFixtureHelpersAttribute : Attribute
{
    public GenerateFixtureHelpersAttribute(string fixtureFile)
    {
        if (fixtureFile is null)
        {
            throw new ArgumentNullException(nameof(fixtureFile));
        }

        this.FixtureFile = fixtureFile;
    }

    public string FixtureFile { get; }
}
