// <copyright file="FixRegister.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes
{
    using Emu.Fixes.FrontierLabs;
    using Emu.Fixes.OpenEcoacoustics;

    public class FixRegister
    {
        private readonly IServiceProvider provider;

        public FixRegister(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public static OperationInfo[] All { get; } = new[]
        {
            //FileNameDateStampInvalid.Metadata,
            EmptyFile.Metadata,
            PreAllocatedHeader.Metadata,
            SpaceInDatestamp.Metadata,
            MetadataDurationBug.Metadata,
            IncorrectDataSize.Metadata,
        };

        public IFixOperation Resolve(WellKnownProblem problem)
        {
            var fix = All.First(x => x.Problem == problem);
            return this.GetFix(fix.FixClass);
        }

        public IFixOperation Resolve(string problemId)
        {
            var fix = All.First(x => x.Problem.Id == problemId);
            return this.GetFix(fix.FixClass);
        }

        public ICheckOperation ResolveCheck(string problemId)
        {
            var fix = All.First(x => x.Problem.Id == problemId);
            return this.GetCheck(fix.FixClass);
        }

        public IEnumerable<IFixOperation> ResolveAll()
        {
            return All.Select(x => this.GetFix(x.FixClass));
        }

        public IEnumerable<ICheckOperation> ResolveAllChecks()
        {
            return All.Select(x => this.GetCheck(x.FixClass));
        }

        private IFixOperation GetFix(Type fixClass) => (IFixOperation)this.provider.GetService(fixClass);

        private ICheckOperation GetCheck(Type fixClass) => (ICheckOperation)this.provider.GetService(fixClass);
    }
}
