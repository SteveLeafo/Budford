namespace Budford.Tools
{
    public abstract class Tool
    {
        protected Tool(Model.Model model)
        {
            Model = model;
        }

        protected Model.Model Model { get; set; }

        public abstract bool Execute();
    }
}
