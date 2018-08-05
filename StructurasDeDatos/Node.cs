using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StructurasDeDatos
{
    public class Node<TKey, TData> where TKey : IComparable<TKey>
    {
        //llave, dato, hijos, padre, Orden, 
        // public TKeys MasterKey { get; set; }
        public List<TKey> NodeKeys { get; set; }
        public List<TData> NodeData { get; set; }
        public int Position { get; set; }
        public List<int> Sons { get; set; } //positions of memory
        public int Father { get; set; } //position of memory
        public int Order { get; set; }
        private int MaxlengthKey;
        private int Count;

        public Node(int order, int position, int maxlengthKey)
        {
            NodeKeys = new List<TKey>();
            NodeData = new List<TData>();
            Sons = new List<int>();

            Order = order;
            Count = 0;
            Father = Factory.makeNull();
            Position = position;
            MaxlengthKey = maxlengthKey;

            ClearSons();
        }

        public void ClearSons()
        {
            if (Sons.Count != 0)
            {
                for (int i = 0; i < Sons.Count; i++) //The order specificate count sons maximun could have the node, this spaces fill it with "nulls"
                {
                    //solo llene la lista de los hijos y padres porque no se como llenar la del nodo ): en realidad ya no sé xD lo tengo que pensar más a fondo ntt
                    Sons[i] = Factory.makeNull();
                }
            }
            else
            {
                for (int i = 0; i < Order; i++) //The order specificate count sons maximun could have the node, this spaces fill it with "nulls"
                {
                    //solo llene la lista de los hijos y padres porque no se como llenar la del nodo ): en realidad ya no sé xD lo tengo que pensar más a fondo ntt
                    Sons.Add(Factory.makeNull());
                }
            }
        }

        public int CompareTo(TKey OtherKey, int index)
        {
            int m = NodeKeys[index].CompareTo(OtherKey);
            return m;
            //  return NodeKeys[index].CompareTo(OtherKey);
        }

        public bool IsLeaf()
        {
            int sNull = Factory.makeNull();

            for (int i = 0; i < Sons.Count; i++)
            {
                if (Sons[i] != sNull) { return false; } //if anything doesn't have value like null, this node have son, It doesn't sheet
            }
            return true;
        }

        public bool IsFull()
        {
            return NodeData.Count == Order - 1;
        }

        public TData GetValueOfIndex(int index)
        {
            return NodeData[index];
        }

        public int GetNodeOfIndex(int index)
        {
            return Sons[index];
        }

        public int GetLastSon()
        {
            for (int i = Sons.Count - 1; i >= 0; i--)
            {
                if (Sons[i] != Factory.makeNull())
                {
                    return Sons[i];
                }
            }
            return Factory.makeNull();
        }

        public void InsertSonFirst(int son)
        {
            for (int i = Sons.Count - 1; i > 0; i--)
            {
                Sons[i] = Sons[i - 1];
            }
            Sons[0] = son;
        }
        public void InsertSonLast(int son)
        {
            for (int i = 0; i < Sons.Count; i++)
            {
                if (Sons[i] == Factory.makeNull())
                {
                    Sons[i] = son;
                    return;
                }
            }
        }

        public TKey GetKeyOfIndex(int index)
        {
            return NodeKeys[index];
        }

        public void DeleteDataOfKey(TKey key)
        {
            int index = NodeKeys.IndexOf(key);
            NodeKeys.Remove(key);
            NodeData.RemoveAt(index);
        }
        public void InsertInOrder(TKey key, TData data, int index)
        {
            NodeData.Add(default(TData));
            NodeKeys.Add(default(TKey));

            for (int i = NodeKeys.Count - 1; i > index; i--)
            {
                NodeKeys[i] = NodeKeys[i - 1];
                NodeData[i] = NodeData[i - 1];
            }

            NodeKeys[index] = key;
            NodeData[index] = data;
        }

        public void Insert(TKey key, TData data)
        {
            int Position = AproximatePosition(key);
            if (Position == NodeKeys.Count)
            {
                NodeKeys.Add(key);
                NodeData.Add(data);
            }
            else
            {
                NodeKeys.Add(default(TKey));
                NodeData.Add(default(TData));
                for (int i = NodeKeys.Count - 1; i > Position; i--)
                {
                    NodeKeys[i] = NodeKeys[i - 1];
                    NodeData[i] = NodeData[i - 1];
                }
                NodeKeys[Position] = key;
                NodeData[Position] = data;
                FixSons(Position);
            }
        }

        public bool IsSonsFull()
        {
            for (int i = 0; i < Sons.Count; i++)
            {
                if (Sons[i] == Factory.makeNull())
                {
                    return false;
                }
            }
            return true;
        }

        public void FixSons(int positionsToMove)
        {
            if (!IsSonsFull())
            {
                positionsToMove++;
                for (int i = Sons.Count - 1; i > positionsToMove; i--)
                {
                    Sons[i] = Sons[i - 1];
                }
                Sons[positionsToMove] = Factory.makeNull();
                Count++;
            }
        }

        public int AproximatePosition(TKey KeyToCompare)
        {
            for (int i = 0; i < NodeKeys.Count; i++)
            {
                if (NodeKeys[i].CompareTo(KeyToCompare) == 1)
                {
                    return i;
                }
            }
            return NodeKeys.Count;
        }

        public TData UpLevel(ref TKey key)
        {
            TData data = NodeData[(Order / 2)];
            NodeData.Remove(data);
            key = NodeKeys[(Order / 2)];
            NodeKeys.Remove(key);
            return data;
        }

        public TData UpLevel(ref TKey key, ref int indexRemoved)
        {
            TData data = NodeData[(Order / 2)];
            NodeData.Remove(data);
            key = NodeKeys[(Order / 2)];
            indexRemoved = NodeKeys.IndexOf(key);
            NodeKeys.Remove(key);
            return data;
        }

        public void InsertGroupOfSons(List<int> GroupSons, int begin, int PositionToinsert)
        {
            begin++;
            for (int i = 0; i < GroupSons.Count; i++)
            {
                Sons.Add(Factory.makeNull());
            }
            int temporal = 0;
            int GroupCount = 0;
            for (int i = begin; i < Sons.Count - 1; i++)
            {
                temporal = Sons[i];
                Sons[i] = GroupSons[GroupCount];
                GroupSons[GroupCount] = temporal;
                if (GroupCount == GroupSons.Count)
                {
                    GroupCount = 0;
                }
            }
            Sons[Count - 1] = temporal;

            Sons[begin] = PositionToinsert;
        }

        public List<TKey> SonsAndValuesToBrother(TKey keyToComparate, ref List<TData> data, ref List<int> sons)
        {
            int condition = NodeKeys.Count;
            List<TKey> Maximuns = new List<TKey>();
            for (int i = 0; i < condition; i++)
            {
                if (NodeKeys[i].CompareTo(keyToComparate) == 1)
                {
                    Maximuns.Add(NodeKeys[i]);
                    //  NodeKeys.Remove(NodeKeys[i]);
                    data.Add(NodeData[i]);
                    // NodeData.Remove(NodeData[i]);
                    sons.Add(Sons[i]);
                    Sons[i] = Factory.makeNull();
                }


            }

            sons.Add(Sons[condition]);
            Sons[condition] = Factory.makeNull();

            for (int i = 0; i < Maximuns.Count; i++)
            {
                NodeKeys.Remove(Maximuns[i]);
                NodeData.Remove(data[i]);
            }

            return Maximuns;
        }


        public List<TKey> SonsAndValuesToCurrent(TKey keyToComparate, ref List<TData> data, ref List<int> sons)
        {
            int condition = NodeKeys.Count;
            List<TKey> Maximuns = new List<TKey>();
            for (int i = 0; i < condition; i++)
            {
                if (NodeKeys[i].CompareTo(keyToComparate) == 1)
                {
                    Maximuns.Add(NodeKeys[i]);
                    //  NodeKeys.Remove(NodeKeys[i]);
                    data.Add(NodeData[i]);
                    // NodeData.Remove(NodeData[i]);
                    sons.Add(Sons[i + 1]);
                    Sons[i + 1] = Factory.makeNull();
                }


            }

            for (int i = 0; i < Maximuns.Count; i++)
            {
                NodeKeys.Remove(Maximuns[i]);
                NodeData.Remove(data[i]);
            }

            return Maximuns;
        }

        public void InsertSons(int position, int directionSon)
        {
            if (Sons[position] != Factory.makeNull())
            {
                //se corren los hijos

                for (int i = Sons.Count - 1; i > position; i--)
                {
                    Sons[i] = Sons[i - 1];
                }
            }
            Sons[position] = directionSon;
        }

        public void Clear()
        {
            ClearSons();
            NodeData.Clear();
            NodeKeys.Clear();
            Father = Factory.makeNull();
        }
    }
}
  
