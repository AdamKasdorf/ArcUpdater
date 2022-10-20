using System.CommandLine;

namespace ArcUpdater.CommandLine
{
    internal class DeleteOption : Option<bool>
    {
        private const string OptionName = "--del";
        private const string OptionDescription = "Specify that --update and --clean commands should overwrite or delete existing assemblies rather than move them to the Recycle Bin.";

        public DeleteOption()
            : base(OptionName, OptionDescription)
        {
            // Does not allow true or false to be explicitly provided.
            // Inclusion of --del option simply sets value to true.
            Arity = ArgumentArity.Zero;
        }
    }
}
