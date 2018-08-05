using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructurasDeDatos
{
    public class B_Tree<TKey, TData> where TKey : IComparable<TKey>
    {
        private FileStream TreeFile; //archivo en el que se lee y escribe, se crea en el constructor de esta clase.
        private List<int> AvailableSpaces;
        int Order { get; set; }
        int Count { get; set; }
        int Height { get; set; }
        public int LastPosition { get; set; }
        public Node<TKey, TData> Root { get; private set; }
        private string Header;
        private int MaxLengthKey;
        private string FilePath;

        public delegate TKey ConvertStringToKey(string s);
        public delegate TData ConvertStringToData(string s);
        public delegate IList<string> GetData(TData data);
        public delegate IList<int> GetDataLength();

        public ConvertStringToKey ConverterStringToTkey;
        public ConvertStringToData ConverterStringToTData;
        public GetData ReturnData;
        public GetDataLength ReturnDataLength;


        public B_Tree(int TreeOrder, string TreeFileName, string TreeFilePath, ConvertStringToKey KeyConverter, ConvertStringToData StringConverter, GetData DataConverter, int KeyMaxLength, GetDataLength DataMaxLength)
        {
            Root = null;
            Order = TreeOrder;
            Count = 0;
            Height = 0;
            LastPosition = 0;
            FilePath = TreeFilePath + TreeFileName;
            TreeFile = File.Create(FilePath); //the file is created in debug folder.
            AvailableSpaces = new List<int>();

            Header = Factory.makeHeader(Factory.makeNull().ToString(), Factory.FixPositionsSize(0), Order, 0) + Environment.NewLine;
            TreeFile.Write(ConvertStringTo_ByteChain(Header), 0, Header.Length);
            ConverterStringToTkey = KeyConverter;
            ConverterStringToTData = StringConverter;
            ReturnData = DataConverter;

            MaxLengthKey = KeyMaxLength;
            ReturnDataLength = DataMaxLength;
            DisposeFile();
        }

        public B_Tree(string TreeFilePath, string TreeFileName, ConvertStringToKey KeyConverter, ConvertStringToData StringConverter, GetData DataConverter, int KeyMaxLength, GetDataLength DataMaxLength)
        {
            FilePath = TreeFilePath + TreeFileName;
            ConverterStringToTkey = KeyConverter;
            ConverterStringToTData = StringConverter;
            ReturnData = DataConverter;
            MaxLengthKey = KeyMaxLength;
            ReturnDataLength = DataMaxLength;

            StreamReader reader = new StreamReader(TreeFilePath + TreeFileName);
            int rootPosition = int.Parse(reader.ReadLine());
            LastPosition = int.Parse(reader.ReadLine());
            reader.ReadLine();
            Order = int.Parse(reader.ReadLine());
            Count = 0;
            Height = int.Parse(reader.ReadLine());
            AvailableSpaces = new List<int>();
            reader.Close();

            TreeFile = File.Open(FilePath, FileMode.Open);
            Root = AccessToNode(rootPosition);


        }

        //------------------------------------------------>>>>> Insert

        public void Insert(TKey newKey, TData newData)
        {
            DisposeFile();
            TreeFile = File.Open(FilePath, FileMode.Open);
            if (Root == null)
            {
                Node<TKey, TData> newNode = NewNode();
                newNode.Insert(newKey, newData);
                Root = newNode;
                TreeFile.Seek(SkipHeader(), SeekOrigin.Begin);
                TreeFile.Write(ConvertStringTo_ByteChain(Print(Root)), 0, Print(Root).Length);
                Count++;
                Height++;

            }
            else
            {
                InsertRecursive(newKey, newData, Root);
            }

            //despues de terminar el insert o el delete actualizar el header en el archivo
            TreeFile.Seek(0, SeekOrigin.Begin);
            if (Root == null)
            {
                Header = Factory.makeHeader(Factory.makeNull().ToString(), Factory.FixPositionsSize(LastPosition), Order, Height);
            }
            else
            {
                Header = Factory.makeHeader(Factory.FixPositionsSize(Root.Position), Factory.FixPositionsSize(LastPosition), Order, Height);
            }
            TreeFile.Write(ConvertStringTo_ByteChain(Header), 0, Header.Length);
            DisposeFile();
        }

        private void InsertRecursive(TKey newKey, TData newData, Node<TKey, TData> Current)
        {
            if (Current.IsLeaf())
            {
                if (!Current.IsFull()) //si es hoja y no esta lleno, solo inserto
                {
                    Current.Insert(newKey, newData);
                    ActualizeNode(Current);
                }
                else
                {
                    //significa que se tiene que insertar allí pero esta lleno
                    Node<TKey, TData> aux = AccessToNode(Current.Father);
                    InsertPop(Current, newData, newKey, Position(aux, newKey));
                }
            }
            else
            {
                //se traslada a otro nodo


                if (newKey.CompareTo(Current.NodeKeys[0]) == -1) //si es menor voy a la primera posición de los hijos
                {
                    InsertRecursive(newKey, newData, AccessToNode(Current.Sons[0])); //access to node devuelve un string string
                }
                else if (newKey.CompareTo(Current.NodeKeys[0]) == 1) //es mayor, veo en que posicion insertarlo
                {
                    int index = GetIndexToInsertData(Current, newKey);
                    InsertRecursive(newKey, newData, AccessToNode(Current.Sons[index]));
                }
                else
                {
                    Count--;
                }
            }
        }

        private Node<TKey, TData> InsertPop(Node<TKey, TData> Current, TData DataPop, TKey KeyPop, int myPosition)
        {
            if (Current.IsFull())
            {
                Current.Insert(KeyPop, DataPop);
                DataPop = Current.UpLevel(ref KeyPop);


                // Create a new node to separate the current node and load the father.
                Node<TKey, TData> Brother = NewNode();
                Node<TKey, TData> Father = AccessToNode(Current.Father);


                // Changing father property on Brother node
                Brother.Father = Current.Father;



                //Actualize all nodes    
                ActualizeNode(Brother);
                ActualizeNode(Current);


                if (Father == null)
                {

                    //create a new node that is gonna be the new root too.
                    Node<TKey, TData> newRoot = NewNode();
                    newRoot.Insert(KeyPop, DataPop);
                    newRoot.InsertSons(0, Current.Position);
                    newRoot.InsertSons(1, Brother.Position);
                    Root = newRoot;
                    Current.Father = newRoot.Position;
                    Brother.Father = newRoot.Position;
                    ActualizeNode(Root);
                    ActualizeNode(Current);
                    ActualizeNode(Brother);
                    Height++;


                    if (myPosition < 0)
                    {
                        // Extracting and moving the data and sons. 
                        List<int> leftSon = new List<int>();
                        List<TData> nData = new List<TData>();
                        Brother.NodeKeys = Current.SonsAndValuesToBrother(KeyPop, ref nData, ref leftSon);
                        Brother.NodeData = nData;
                        ActualizeGroupOfSons(Current, Current.Sons);
                        ActualizeGroupOfSons(Brother, leftSon);
                        ActualizeNode(Current);
                        ActualizeNode(Brother);
                        return Current;
                    }
                    else
                    {
                        // Extracting and moving the data and sons. 
                        List<int> leftSon = new List<int>();
                        List<TData> nData = new List<TData>();
                        Brother.NodeKeys = Current.SonsAndValuesToCurrent(KeyPop, ref nData, ref leftSon);
                        Brother.NodeData = nData;
                        ActualizeGroupOfSons(Current, Current.Sons);
                        ActualizeGroupOfSons(Brother, leftSon);
                        ActualizeNode(Current);
                        ActualizeNode(Brother);
                        return Brother;
                    }
                }
                else
                {

                    List<int> leftSon = new List<int>();
                    List<TData> nData = new List<TData>();
                    Node<TKey, TData> auxFather = InsertPop(Father, DataPop, KeyPop, Position(Father, KeyPop));

                    // Getting aproximate position to Father's new son (brother)
                    int aproximatePosition = auxFather.AproximatePosition(KeyPop);

                    // Inserting the son where it should be on the father.
                    auxFather.InsertSons(aproximatePosition, Brother.Position);
                    Brother.Father = auxFather.Position;

                    Current = AccessToNode(Current.Position);
                    Father = AccessToNode(Father.Position);

                    if (myPosition < 0) // left (or middle left-side) node separated
                    {
                        // Extracting and moving the data and sons. 
                        Brother.NodeKeys = Current.SonsAndValuesToBrother(KeyPop, ref nData, ref leftSon);
                        Brother.NodeData = nData;
                        ActualizeGroupOfSons(Brother, leftSon);

                        ActualizeNode(auxFather);
                        ActualizeNode(Current);
                        ActualizeNode(Brother);
                        ActualizeNode(Father);

                        ActualizeGroupOfSons(Father, Father.Sons);
                        ActualizeGroupOfSons(auxFather, auxFather.Sons);
                        return Current;
                    }
                    else if (myPosition > 0)// right (or middle-right) node separated
                    {
                        // Extracting and moving the data and sons. 
                        Brother.NodeKeys = Current.SonsAndValuesToCurrent(KeyPop, ref nData, ref leftSon);
                        Brother.NodeData = nData;
                        ActualizeGroupOfSons(Brother, leftSon);


                        Brother.Father = auxFather.Position;
                        ActualizeNode(auxFather);
                        ActualizeNode(Current);
                        ActualizeNode(Brother);
                        ActualizeNode(Father);

                        ActualizeGroupOfSons(Father, Father.Sons);
                        ActualizeGroupOfSons(auxFather, auxFather.Sons);


                        auxFather = AccessToNode(auxFather.Position);
                        Current = AccessToNode(Current.Position);
                        Brother = AccessToNode(Brother.Position);
                        Father = AccessToNode(Father.Position);

                        return Brother;
                    }
                    else //or middle-right node separated
                    {
                        // Extracting and moving the data and sons. 
                        Brother.NodeKeys = Current.SonsAndValuesToCurrent(KeyPop, ref nData, ref leftSon);
                        Brother.NodeData = nData;
                        ActualizeGroupOfSons(Brother, leftSon);

                        ActualizeNode(auxFather);
                        ActualizeNode(Current);
                        ActualizeNode(Brother);
                        ActualizeNode(Father);
                        ActualizeGroupOfSons(Father, Father.Sons);
                        ActualizeGroupOfSons(auxFather, auxFather.Sons);

                        return Brother;
                    }
                }


            }
            else //solo inserta, si algun cambio se hizo en el root procura que sean guardados.
            {
                Current.Insert(KeyPop, DataPop);
                ActualizeNode(Current);
                if (Current.Position == Root.Position)
                {
                    Root = Current;
                }
                return Current;
            }

        }

        //------------------------------------------------>>>>> Search

        public Node<TKey, TData> Search(TKey key)
        {
            DisposeFile();
            TreeFile = File.Open(FilePath, FileMode.Open);
            if (Root != null)
            {
                return SearchRecursive(Root, key);
            }
            DisposeFile();
            return null;
        }

        private Node<TKey, TData> SearchRecursive(Node<TKey, TData> currentNode, TKey key)
        {
            if (currentNode.NodeKeys.Exists(x => key.CompareTo(x) == 0))
            {
                return currentNode;
            }
            else
            {
                int index = currentNode.AproximatePosition(key);
                if (currentNode.GetNodeOfIndex(index) == Factory.makeNull())
                {
                    return null;
                }
                return SearchRecursive(AccessToNode(currentNode.Sons[index]), key);
            }
        }

        public List<string> InOrder(Node<TKey, TData> currentNode, List<string> data)
        {
            DisposeFile();
            TreeFile = File.Open(FilePath, FileMode.Open);
            data = InOrderRecursive(currentNode, data);
            DisposeFile();
            return data;
        }
        private List<string> InOrderRecursive(Node<TKey, TData> currentNode, List<string> data)
        {
            if (currentNode == null)
            {
                return data;
            }
            if (currentNode.IsLeaf())
            {
                for (int i = 0; i < currentNode.NodeData.Count; i++)
                {
                    IList<string> values = ReturnData(currentNode.NodeData[i]);
                    string line = "";
                    for (int j = 0; j < values.Count; j++)
                    {
                        line += values[j] + "|";
                    }
                    data.Add(line.Remove(line.Length - 1) + "\n");
                }
                return data;
            }
            else
            {
                for (int i = 0; i < currentNode.Sons.Count; i++)
                {
                    data.Concat(InOrderRecursive(AccessToNode(currentNode.Sons[i]), data));
                    if (i < currentNode.NodeData.Count)
                    {
                        IList<string> values = ReturnData(currentNode.NodeData[i]);
                        string line = "";
                        for (int j = 0; j < values.Count; j++)
                        {
                            line += values[j] + "|";
                        }
                        data.Add(line.Remove(line.Length - 1) + "\n");
                    }
                }
                return data;
            }
        }

        //------------------------------------------------>>>>> Update

        public void Update(List<TData> newData, TKey key)
        {
            Node<TKey, TData> currentNode = Search(key);
            DisposeFile();
            TreeFile = File.Open(FilePath, FileMode.Open);
            if (currentNode != null)
            {
                currentNode.NodeData = newData;
                ActualizeNode(currentNode);
            }
            DisposeFile();
        }

        //------------------------------------------------>>>>> Delete

        public void Clear()
        {
            DisposeFile();
            TreeFile = File.Create(TreeFile.Name);
            TreeFile.Seek(0, SeekOrigin.Begin);
            Root = null;
            Count = 0;
            Height = 0;
            LastPosition = 0;
            AvailableSpaces = new List<int>();

            Header = Factory.makeHeader(Factory.makeNull().ToString(), Factory.FixPositionsSize(LastPosition), Order, Height);
            TreeFile.Write(ConvertStringTo_ByteChain(Header), 0, Header.Length);
            DisposeFile();
        }

        public void Delete(TKey key)
        {
            Node<TKey, TData> currentNode = Search(key);

            DisposeFile();
            TreeFile = File.Open(FilePath, FileMode.Open);

            if (currentNode != null)
            {
                DeleteRecursive(currentNode, key, (!currentNode.IsLeaf()));
            }
            TreeFile.Flush();
            DisposeFile();
        }

        private Node<TKey, TData> DeleteRecursive(Node<TKey, TData> Current, TKey key, bool Internal)
        {
            if (Internal)
            {
                Current = SwapToDelete(Current, key);
                return DeleteRecursive(Current, key, false);
            }
            else
            {
                //Perfect case
                if (Current.NodeKeys.Count > (Order % 2 == 0 ? Order / 2 - 1 : Order / 2) || Current.Father == Factory.makeNull())
                {
                    return DeletePerfectCase(Current, key, Internal);
                }
                else
                {
                    // It needs a brother to borrow a value.
                    Node<TKey, TData> Father = AccessToNode(Current.Father);
                    Node<TKey, TData> Brother = BrotherToBorrow(Father, Current);

                    if (Brother != null) // current has a brother to borrow.
                    {
                        return DeleteNormalCase(Current, Father, Brother, key, Internal);
                    }
                    else // Current node has not a brother to borrow. Bad case.
                    {
                        return DeleteBadCase(Current, Father, Brother, key, Internal);
                    }
                }
            }
        }

        private Node<TKey, TData> DeletePerfectCase(Node<TKey, TData> Current, TKey key, bool Internal)
        {
            Current.DeleteDataOfKey(key);
            if (Current.Father == Factory.makeNull() && Current.NodeKeys.Count == 0)
            {
                if (Current.IsLeaf())
                {
                    Current.Father = Factory.makeNull();
                    Root.Clear();
                    ActualizeNode(Root);
                    AvailableSpaces.Add(Root.Position);
                    Root = null;

                    TreeFile.Seek(0, SeekOrigin.Begin);
                    if (Root == null)
                    {
                        Header = Factory.makeHeader(Factory.makeNull().ToString(), Factory.FixPositionsSize(LastPosition), Order, Height);
                    }
                    else
                    {
                        Header = Factory.makeHeader(Factory.FixPositionsSize(Root.Position), Factory.FixPositionsSize(LastPosition), Order, Height);
                    }
                    TreeFile.Write(ConvertStringTo_ByteChain(Header), 0, Header.Length);
                }
                else
                {
                    Node<TKey, TData> leftSon = AccessToNode(Current.Sons[0]);
                    leftSon.Father = Factory.makeNull();
                    ActualizeNode(leftSon);
                    Root.Clear();
                    ActualizeNode(Root);
                    AvailableSpaces.Add(Root.Position);
                    ActualizeGroupOfSons(leftSon, leftSon.Sons);
                    Root = leftSon;
                    Height--;

                    TreeFile.Seek(0, SeekOrigin.Begin);
                    if (Root == null)
                    {
                        Header = Factory.makeHeader(Factory.makeNull().ToString(), Factory.FixPositionsSize(LastPosition), Order, Height);
                    }
                    else
                    {
                        Header = Factory.makeHeader(Factory.FixPositionsSize(Root.Position), Factory.FixPositionsSize(LastPosition), Order, Height);
                    }
                    TreeFile.Write(ConvertStringTo_ByteChain(Header), 0, Header.Length);

                    return leftSon;
                }
            }
            else if (Current.Father == Factory.makeNull())
            {
                // Current is root but it still has ONE key
                Root = Current;
            }
            ActualizeNode(Current);
            return Current;
        }

        private Node<TKey, TData> DeleteNormalCase(Node<TKey, TData> Current, Node<TKey, TData> Father, Node<TKey, TData> Brother, TKey key, bool Internal)
        {
            if (Current.NodeKeys[0].CompareTo(Brother.NodeKeys[0]) == 1) // brother is left node.
            {
                // Values from brother
                TKey brotherKey = Brother.NodeKeys[Brother.NodeKeys.Count - 1];
                TData brotherData = Brother.NodeData[Brother.NodeData.Count - 1];
                int brotherSon = Brother.GetLastSon();

                //delete the son taken from  brother
                if (brotherSon != Factory.makeNull())
                {
                    Brother.Sons.Remove(brotherSon);
                    Brother.Sons.Add(Factory.makeNull());
                }

                //Values from current
                //Tkey currentKey = key
                TData currentData = Current.NodeData[Current.NodeKeys.IndexOf(key)];

                // Get the index of the Key taken from father.
                int index = Father.Sons.IndexOf(Brother.Position);

                //Values from father
                TKey fatherKey = Father.NodeKeys[index];
                TData fatherData = Father.NodeData[index];


                //Inserting in that index the Values taken from brother
                Father.NodeKeys[index] = brotherKey;
                Father.NodeData[index] = brotherData;

                //Remove brothers values.
                Brother.DeleteDataOfKey(brotherKey);

                Current.DeleteDataOfKey(key);
                Current.InsertInOrder(fatherKey, fatherData, 0);

                //Adding the son taken from brother
                Current.InsertSonFirst(brotherSon);

                ActualizeNode(Current);
                ActualizeNode(Brother);
                ActualizeNode(Father);
                ActualizeGroupOfSons(Current, Current.Sons);
                if (Father.Position == Root.Position)
                {
                    Root = Father;
                }
                return Current;
            }
            else // brother is right node.
            {
                // Values from brother
                TKey brotherKey = Brother.NodeKeys[0];
                TData brotherData = Brother.NodeData[0];
                int brotherSon = Brother.Sons[0];

                //delete the son taken from  brother
                if (brotherSon != Factory.makeNull())
                {
                    Brother.Sons.Remove(brotherSon);
                    Brother.Sons.Add(Factory.makeNull());
                }

                //Values from current
                //Tkey currentKey = key
                TData currentData = Current.NodeData[Current.NodeKeys.IndexOf(key)];

                // Get the index of the Key taken from father.
                int index = Father.Sons.IndexOf(Current.Position);

                //Values from father
                TKey fatherKey = Father.NodeKeys[index];
                TData fatherData = Father.NodeData[index];

                //Inserting in index the Values taken from brother
                Father.NodeKeys[index] = brotherKey;
                Father.NodeData[index] = brotherData;

                //Remove brothers values.
                Brother.DeleteDataOfKey(brotherKey);

                //Get the index of the Key deleted on current node
                index = Current.NodeKeys.IndexOf(key);
                Current.DeleteDataOfKey(key);
                Current.NodeKeys.Add(fatherKey);
                Current.NodeData.Add(fatherData);

                //Adding the son taken from brother
                Current.InsertSonLast(brotherSon);

                ActualizeNode(Current);
                ActualizeNode(Brother);
                ActualizeNode(Father);
                ActualizeGroupOfSons(Current, Current.Sons);
                if (Father.Position == Root.Position)
                {
                    Root = Father;
                }
                return Current;
            }
        }

        private Node<TKey, TData> DeleteBadCase(Node<TKey, TData> Current, Node<TKey, TData> Father, Node<TKey, TData> Brother, TKey key, bool Internal)
        {
            Brother = GiveMeMyBrother(Father, Current);
            //Father's key and data
            int index;

            if (Father.Sons.IndexOf(Brother.Position) > Father.Sons.IndexOf(Current.Position)) //brother is right node
            {
                index = Father.Sons.IndexOf(Current.Position);
                // Brother complete values
                List<TKey> brotherKeys = Brother.NodeKeys;
                List<TData> brotherData = Brother.NodeData;
                List<int> brotherSons = Brother.Sons;

                // Clean values before use them
                brotherKeys.RemoveAll(x => Factory.makeNull().Equals(x));
                brotherData.RemoveAll(x => Factory.makeNull().Equals(x));
                brotherSons.RemoveAll(x => Factory.makeNull().Equals(x));

                TKey fatherKey = Father.NodeKeys[index];
                TData fatherData = Father.NodeData[index];
                // DO NOT REMOVE FATHER'S VALUES HERE, THEY MUST BE DELETED USIGN A RECURSIVE CALL TO THIS METHOD.

                // recicle the current node make the things easier to program.
                Current.DeleteDataOfKey(key);
                Current.NodeKeys.Add(fatherKey);
                Current.NodeData.Add(fatherData);

                for (int i = 0; i < brotherSons.Count; i++)
                {
                    if (Current.Sons.IndexOf(Factory.makeNull()) != -1)
                    {
                        Current.Sons[Current.Sons.IndexOf(Factory.makeNull())] = brotherSons[i];
                    }
                }

                for (int i = 0; i < brotherKeys.Count; i++)
                {
                    Current.NodeKeys.Add(brotherKeys[i]);
                    Current.NodeData.Add(brotherData[i]);
                }

                //Delete from father the brother's son reference.
                Father.Sons.Remove(Brother.Position);
                Father.Sons.Add(Factory.makeNull());

                Brother.Clear();

                // Actualize nodes
                ActualizeNode(Father);
                ActualizeNode(Current);
                ActualizeNode(Brother);
                AvailableSpaces.Add(Brother.Position); // brother is now an unused node -> add to Unused nodes list/reference

                Father = DeleteRecursive(AccessToNode(Father.Position), fatherKey, false);
                if (Father.Position != Current.Position)
                {
                    Current.Father = Father.Position;
                    ActualizeNode(Current);
                }
                else
                {
                    Current = Father;
                }
                ActualizeGroupOfSons(Father, Father.Sons);
                ActualizeGroupOfSons(Current, Current.Sons);
                return Current;
            }
            else //brother is left node
            {
                index = Father.Sons.IndexOf(Brother.Position);
                Current.DeleteDataOfKey(key);

                // Brother complete values
                List<TKey> currentKeys = Current.NodeKeys;
                List<TData> currentData = Current.NodeData;
                List<int> currentSons = Current.Sons;

                // Clean values before use them
                currentKeys.RemoveAll(x => Factory.makeNull().Equals(x));
                currentData.RemoveAll(x => Factory.makeNull().Equals(x));
                currentSons.RemoveAll(x => Factory.makeNull().Equals(x));

                TKey fatherKey = Father.NodeKeys[index];
                TData fatherData = Father.NodeData[index];
                // DO NOT REMOVE FATHER'S VALUES HERE, THEY MUST BE DELETED USIGN A RECURSIVE CALL TO THIS METHOD.

                // recicle the brother node make the things easier to program.
                Brother.NodeKeys.Add(fatherKey);
                Brother.NodeData.Add(fatherData);

                for (int i = 0; i < currentSons.Count; i++)
                {
                    if (Brother.Sons.IndexOf(Factory.makeNull()) != -1)
                    {
                        Brother.Sons[Brother.Sons.IndexOf(Factory.makeNull())] = currentSons[i];
                    }
                }

                for (int i = 0; i < currentKeys.Count; i++)
                {
                    Brother.NodeKeys.Add(currentKeys[i]);
                    Brother.NodeData.Add(currentData[i]);
                }

                //Delete from father the current's son reference.
                Father.Sons.Remove(Current.Position);
                Father.Sons.Add(Factory.makeNull());

                // Actualize nodes
                Current.Clear();
                ActualizeNode(Father);
                ActualizeNode(Current);
                ActualizeNode(Brother);
                AvailableSpaces.Add(Current.Position);// current is now an unused node -> add to Unused nodes list/reference

                Father = DeleteRecursive(AccessToNode(Father.Position), fatherKey, false);

                if (Father.Position != Brother.Position)
                {
                    Brother.Father = Father.Position;
                    ActualizeNode(Brother);
                }
                else
                {
                    Brother = Father;
                }
                ActualizeGroupOfSons(Father, Father.Sons);
                ActualizeGroupOfSons(Brother, Brother.Sons);
                return Brother;

            }
        }

        //------------------------------------------------>>>>> Methods and Functions 

        private void AddTextToFile(string text)
        {

            text += Environment.NewLine;
            byte[] info = new UTF8Encoding(true).GetBytes(text);
            TreeFile.Write(info, 0, info.Length);

        }

        private byte[] ConvertStringTo_ByteChain(string text)
        {
            return new UTF8Encoding(true).GetBytes(text);
        }

        private int GetIndexToInsertData(Node<TKey, TData> Current, TKey newkey)
        {
            int index = 0;
            while (newkey.CompareTo(Current.NodeKeys[index]) == 1)
            {
                if (index + 1 == Current.NodeKeys.Count)
                {
                    return index + 1;
                }
                index++;
            }

            return index;
        }

        private int Position(Node<TKey, TData> Father, TKey KeyPop)
        {
            if (Father == null)
            {
                return 1;
            }

            if (Father.AproximatePosition(KeyPop) == 0) //left node
            {
                return -1;
            }
            else if (Father.AproximatePosition(KeyPop) == Father.NodeKeys.Count) // right node
            {
                return 1;
            }
            else if (Father.AproximatePosition(KeyPop) >= Order / 2)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        private void ActualizeGroupOfSons(Node<TKey, TData> SonRight, List<int> SonsForNewNode)
        {
            Node<TKey, TData> littleSon;
            for (int i = 0; i < SonsForNewNode.Count; i++)
            {
                if (SonsForNewNode[i] != Factory.makeNull())
                {
                    SonRight.Sons[i] = SonsForNewNode[i];
                    littleSon = AccessToNode(SonsForNewNode[i]);
                    littleSon.Father = SonRight.Position;
                    ActualizeNode(littleSon);
                }
            }
            ActualizeNode(SonRight);
        }

        private Node<TKey, TData> NewNode()
        {
            Node<TKey, TData> NodeAvailable;

            if (AvailableSpaces.Count == 0)
            {
                return NodeAvailable = new Node<TKey, TData>(Order, LastPosition++, MaxLengthKey);
            }
            else
            {
                int p = AvailableSpaces.Last(); //se busca una posicion disponible
                AvailableSpaces.Remove(p); //se remueve esa posicion porque ya no esta disponible
                Node<TKey, TData> n = AccessToNode(p); //se llama esa posición del archivo
                n.Clear(); //Se limpia por si tiene algo de contenido
                return n;
            }
        }

        private Node<TKey, TData> BrotherToBorrow(Node<TKey, TData> Father, Node<TKey, TData> Current)
        {
            int index = Father.Sons.IndexOf(Current.Position);
            List<int> sons = new List<int>();
            for (int i = 0; i < Father.Sons.Count; i++)
            {
                if (Father.Sons[i] != Factory.makeNull())
                {
                    sons.Add(Father.Sons[i]);
                }
            }

            if (index == 0)
            {
                Node<TKey, TData> right = AccessToNode(sons[index + 1]);
                if (right.NodeKeys.Count > (Order % 2 == 0 ? Order / 2 - 1 : Order / 2))
                {
                    return right;
                }
            }
            else if (index == sons.Count - 1)
            {
                Node<TKey, TData> left = AccessToNode(sons[index - 1]);
                if (left.NodeKeys.Count > (Order % 2 == 0 ? Order / 2 - 1 : Order / 2))
                {
                    return left;
                }
            }
            else
            {
                Node<TKey, TData> left = AccessToNode(sons[index - 1]);
                Node<TKey, TData> right = AccessToNode(sons[index + 1]);
                if (left != null && right != null)
                {
                    if (left.NodeKeys.Count >= right.NodeKeys.Count)
                    {
                        if (left.NodeKeys.Count > (Order % 2 == 0 ? Order / 2 - 1 : Order / 2) && left.Position != Current.Position)
                        {
                            return left;
                        }
                    }
                    else
                    {
                        if (right.NodeKeys.Count > (Order % 2 == 0 ? Order / 2 - 1 : Order / 2) && right.Position != Current.Position)
                        {
                            return right;
                        }
                    }
                }
            }

            return null;
        }

        private Node<TKey, TData> GiveMeMyBrother(Node<TKey, TData> Father, Node<TKey, TData> Current)
        {
            int index = Father.Sons.IndexOf(Current.Position);
            List<int> sons = new List<int>();
            for (int i = 0; i < Father.Sons.Count; i++)
            {
                if (Father.Sons[i] != Factory.makeNull())
                {
                    sons.Add(Father.Sons[i]);
                }
            }

            if (index == 0)
            {
                Node<TKey, TData> right = AccessToNode(sons[index + 1]);
                return right;
            }
            else if (index == sons.Count - 1)
            {
                Node<TKey, TData> left = AccessToNode(sons[index - 1]);
                return left;
            }
            else
            {
                Node<TKey, TData> left = AccessToNode(sons[index - 1]);
                Node<TKey, TData> right = AccessToNode(sons[index + 1]);
                if (left.Position != Current.Position)
                {
                    return left;
                }
                if (right.Position != Current.Position)
                {
                    return right;
                }
            }
            return null; // This means that the tree file is corrupted.
        }

        private Node<TKey, TData> SwapToDelete(Node<TKey, TData> currentNode, TKey key)
        {
            // Since delete actions only can be performed on a leaf node, anytime the value to delete could be on a internal node, the tree
            // ought to swap th value to delete with the minimal left value from right-son of current node or the maximun right value from left-son of current node.
            int aproximatePosition = currentNode.AproximatePosition(key) - 1;

            Node<TKey, TData> sonNode = LastNodeOnRight(AccessToNode(currentNode.Sons[aproximatePosition]));

            // saving data to change to right (depends of the brother)
            TData currentData = currentNode.GetValueOfIndex(aproximatePosition);
            TKey currentKey = currentNode.GetKeyOfIndex(aproximatePosition);
            TData sonData = sonNode.GetValueOfIndex(sonNode.NodeKeys.Count - 1);
            TKey sonKey = sonNode.GetKeyOfIndex(sonNode.NodeKeys.Count - 1);

            //swap values to continue deleting.

            currentNode.NodeKeys[aproximatePosition] = sonKey;
            currentNode.NodeData[aproximatePosition] = sonData;

            sonNode.NodeKeys[sonNode.NodeKeys.Count - 1] = currentKey;
            sonNode.NodeData[sonNode.NodeKeys.Count - 1] = currentData;

            ActualizeNode(currentNode);
            ActualizeNode(sonNode);
            return sonNode;
        }

        private Node<TKey, TData> LastNodeOnRight(Node<TKey, TData> LeftSonOfCurrentNode)
        {
            if (LeftSonOfCurrentNode.Sons[0] == Factory.makeNull())
            {
                return LeftSonOfCurrentNode;
            }
            else
            {
                for (int j = LeftSonOfCurrentNode.Sons.Count - 1; j >= 0; j--)
                {
                    if (LeftSonOfCurrentNode.Sons[j] != Factory.makeNull())
                    {
                        return LastNodeOnRight(AccessToNode(LeftSonOfCurrentNode.Sons[j]));
                    }
                }
                return null;
            }
        }

        private int SkipCompleteLine(int LengthOfData, int LengthOfKeys)
        {
            int NullLength = Factory.CountCharactersOfANull();
            int MaxKeys = (Order - 1);
            return 1 + ((NullLength + 1) + (MaxKeys * LengthOfKeys + MaxKeys) + (MaxKeys * LengthOfData + MaxKeys) + (NullLength + 1) + (NullLength * Order + Order)) + 1;
        }

        private int SkipHeader()
        {
            return (Factory.CountCharactersOfANull() * 5) + (5 * 2);
        }

        public Node<TKey, TData> AccessToNode(int position)
        {
            if (position == Factory.makeNull())
            {
                return null;
            }
            int jumps = 0;
            jumps = jumpLinesAndHeader(position);

            StreamReader reader = new StreamReader(TreeFile);
            reader.BaseStream.Seek(jumps, SeekOrigin.Begin);
            string[] NodeLine = reader.ReadLine().Split('|');
            Node<TKey, TData> AuxNode = new Node<TKey, TData>(Order, int.Parse(NodeLine[0]), MaxLengthKey); //Its Positiom
            //all the keys
            for (int i = 1; i < Order; i++)
            {
                if (NodeLine[i] != Factory.MakeNullKey(MaxLengthKey))
                {
                    AuxNode.NodeKeys.Add(ConverterStringToTkey(Factory.ReturnOriginalKey(NodeLine[i])));
                }
            }

            //all the data
            int condition = (Order);
            for (int i = condition; i < condition + (Order - 1); i++)
            {
                if (NodeLine[i] != Factory.MakeNullData(GetMaxLenghtData(ReturnDataLength())) || NodeLine[i].Contains('#'))
                {
                    AuxNode.NodeData.Add(ConverterStringToTData(Factory.ReturnOriginalData(NodeLine[i])));
                }
            }

            condition += (Order - 1);
            //father position
            AuxNode.Father = int.Parse(NodeLine[condition]);

            condition += 1;
            //al the sons
            int c = 0;
            for (int i = condition; i < condition + Order; i++)
            {
                // AuxNode.Son_Add(int.Parse(NodeLine[i]));
                AuxNode.Sons[c++] = int.Parse(NodeLine[i]);
            }
            return AuxNode;
        }

        private void ActualizeNode(Node<TKey, TData> nNode)
        {
            int jumps = jumpLinesAndHeader(nNode.Position);
            TreeFile.Seek(jumps, SeekOrigin.Begin);
            TreeFile.Write(ConvertStringTo_ByteChain(Print(nNode)), 0, Print(nNode).Length);
            TreeFile.Flush();
        }

        private int jumpLinesAndHeader(int PositionToGo)
        {
            return SkipHeader() + (SkipCompleteLine(GetMaxLenghtData(ReturnDataLength()), MaxLengthKey) * PositionToGo); //valores quemados, tomar en cuenta para el proyecto
        }

        private int GetMaxLenghtData(IEnumerable<int> data)
        {
            int i = 0;
            foreach (var item in data)
            {
                i += item + 1;
            }
            return i == 0 ? i : i - 1;
        }

        public string Print(Node<TKey, TData> Current)
        {
            string chain = "";
            chain += Factory.FixPositionsSize(Current.Position) + "|" + stringOfList(Current.NodeKeys) + stringOfList(Current.NodeData) + Factory.FixPositionsSize(Current.Father) + "|" + stringOfList(Current.Sons) + Environment.NewLine;
            return chain;
        }

        private string stringOfList(List<int> s)
        {
            string chain = "";

            for (int i = 0; i < Order; i++)
            {
                if (i < s.Count)
                {
                    chain += Factory.FixPositionsSize(s[i]) + "|";
                }
                else
                {
                    chain += Factory.makeNull() + "|";
                }
            }
            return chain;
        }

        private string stringOfList(List<TKey> s)
        {
            string chain = "";

            for (int i = 0; i < Order - 1; i++)
            {
                if (i < s.Count)
                {
                    chain += Factory.FixKeySize(s[i].ToString(), MaxLengthKey) + "|";
                }
                else
                {
                    chain += Factory.MakeNullKey(MaxLengthKey) + "|";
                }
            }
            return chain;
        }

        private string stringOfList(List<TData> s)
        {
            string chain = "";
            for (int i = 0; i < Order - 1; i++)
            {
                string aux = "";
                if (i < s.Count)
                {
                    IList<string> valuesFromData = ReturnData(s[i]);
                    IList<int> valuesLenght = ReturnDataLength();
                    for (int j = 0; j < valuesFromData.Count; j++)
                    {
                        aux += Factory.FixDataSize(valuesFromData[j], valuesLenght[j]) + "#";
                    }
                    chain += aux.Remove(aux.Length - 1) + "|";
                }
                else
                {
                    IEnumerable<int> valuesFromData = ReturnDataLength();
                    foreach (var item in valuesFromData)
                    {
                        aux += Factory.MakeNullData(item) + "~";
                    }
                    chain += aux.Remove(aux.Length - 1) + "|";
                }
            }
            return chain;
        }

        private void DisposeFile()
        {
            if (TreeFile != null)
            {
                TreeFile.Dispose();
            }
        }
    }


}
