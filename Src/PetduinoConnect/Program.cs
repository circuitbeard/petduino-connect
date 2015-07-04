namespace PetduinoConnect
{
    class Program
    {
        static void Main()
        {
            var sketch = new PetduinoConnect();
            var loop = true;

            ConsoleUtils.ConsoleClose += (o, i) => {
                loop = false;
                sketch.Dispose(); 
            };

            sketch.Setup();
            while (loop)
            {
                sketch.Loop();
            }
        }
    }
}
