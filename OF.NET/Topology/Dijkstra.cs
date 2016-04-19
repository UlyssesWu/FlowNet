using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Reference: https://github.com/mburst/dijkstras-algorithm/

namespace FlowNet.Topology
{
    public class Dijkstra
    {
        readonly Dictionary<string, Dictionary<string, int>> _vertices = new Dictionary<string, Dictionary<string, int>>();

        public void AddVertex(string name, Dictionary<string, int> edges)
        {
            _vertices[name] = edges;
        }

        public bool RemoveVertex(string name)
        {
            return _vertices.Remove(name);
        }

        public bool RemoveLinksBetweenVertex(string name1, string name2)
        {
            if (!_vertices.ContainsKey(name1) || !_vertices.ContainsKey(name2))
            {
                return false;
            }
            return _vertices[name1].Remove(name2) || _vertices[name2].Remove(name1);
        }

        public List<string> FindShortestPath(string start, string finish)
        {
            var previous = new Dictionary<string, string>();
            var distances = new Dictionary<string, int>();
            var nodes = new List<string>();

            List<string> path = null;

            foreach (var vertex in _vertices)
            {
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = int.MaxValue;
                }

                nodes.Add(vertex.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<string>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in _vertices[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }

            return path;
        }
        
    }

    public class Test
    {
        public static void Main()
        {

        }
    }
}
