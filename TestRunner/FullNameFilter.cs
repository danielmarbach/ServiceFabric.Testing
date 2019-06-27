using NUnit.Framework.Interfaces;

namespace TestRunner.NUnit
{
    class FullNameFilter : ITestFilter
    {
        public FullNameFilter(string fullName)
        {
            this.fullName = fullName;
        }

        public TNode ToXml(bool recursive)
        {
            return TNode.FromXml($"<filter><test>{fullName}</test></filter>");
        }

        public TNode AddToXml(TNode parentNode, bool recursive)
        {
            parentNode.AddElement("test", fullName);
            return parentNode;
        }

        public bool Pass(ITest test)
        {
            return Match(test) || MatchParent(test) || MatchDescendant(test);
        }

        public bool IsExplicitMatch(ITest test)
        {
            return Match(test) || MatchDescendant(test);
        }

        bool MatchParent(ITest test)
        {
            return test.Parent != null && (Match(test.Parent) || MatchParent(test.Parent));
        }

        bool Match(ITest test)
        {
            return test.FullName.Equals(fullName);
        }

        private bool MatchDescendant(ITest test)
        {
            if (test.Tests == null)
            {
                return false;
            }

            foreach (var child in test.Tests)
            {
                if (Match(child) || MatchDescendant(child))
                {
                    return true;
                }
            }

            return false;
        }

        string fullName;
    }
}