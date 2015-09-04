using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utility
{
    public static class Serializer
    {
        public static void Serialize(string file, object obj)
        {
            using (var fs = File.Open(file, FileMode.Create))
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(fs, obj);
            }
        }

        public static T Deserialize<T>(string file)
        {
            using (var fs = File.Open(file, FileMode.Open))
            {
                var serializer = new BinaryFormatter();
                return (T)serializer.Deserialize(fs);
            }
        }

        public static object Deserialize(string file)
        {
            return Deserialize<object>(file);
        }
    }
}
