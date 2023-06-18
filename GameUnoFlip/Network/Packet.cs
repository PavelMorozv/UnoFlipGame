using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace Network
{
    [Serializable]
    public class Packet
    {
        private Dictionary<Enum, object> data;

        public Packet()
        {
            data = new Dictionary<Enum, object>();
        }

        public Packet Add<T>(Enum key, T value)
        {
            if (value == null) throw new ArgumentNullException("value");
            data[key] = value;
            return this;
        }

        public T Get<T>(Enum key)
        {
            try
            {
                if (data.ContainsKey(key))
                {
                    object value = data[key];
                    if (value is T)
                    {
                        return (T)value;
                    }
                }
            }
            catch (Exception)
            {
                throw new KeyNotFoundException($"Key '{key}' not found or value type mismatch.");
            }

            return default(T);
        }

        public byte[] Serialize()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                return stream.ToArray();
            }
        }

        public static Packet Deserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                Dictionary<Enum, object> deserializedData = (Dictionary<Enum, object>)formatter.Deserialize(stream);
                Packet packet = new Packet();
                packet.data = deserializedData;
                return packet;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<Enum, object> entry in data)
            {
                string valueString;
                if (entry.Value is Enum)
                {
                    valueString = entry.Value.ToString();
                }
                else
                {
                    valueString = Regex.Unescape(entry.Value.ToString());
                }

                sb.AppendLine($"{entry.Key}: {valueString}");
            }

            return sb.ToString();
        }
    }
}
