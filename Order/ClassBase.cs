using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Data.SqlServerCe;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

//using System.Threading;
using System.Data.Common;

//
namespace Order
{
    
    static  class CreateStr  // 
    {
    public readonly static string CreateOrderstr =
                    "(" +
                    "[Id] BIGINT NOT NULL IDENTITY (1,1) PRIMARY KEY," +           // uniqueidentifier
                    "[NDock] NVARCHAR(20) NOT NULL," +                 // Номер документа
                    "[Summ] REAL NOT NULL," +                          // Сумма заказа
                    "[Status] INT NOT NULL," +                         // Ыефегы заказа

                    // Эти поля для того чтобы не удалять заказ в случае удаления или изменения типа заказа                     
                    "[NameType] NVARCHAR(30) NOT NULL," +              // Наименование Типа Заказа
                    "[Perc] REAL NOT NULL);";// +                      // Процент
                    //"[Comment] NVARCHAR(250) NOT NULL);";            // Коментарий

    public readonly static string CreateTypeOrderStr =
                    "(" +
                    "[Id] BIGINT NOT NULL IDENTITY (1,1)," +           // uniqueidentifier
                    "[Name] NVARCHAR(30) NOT NULL," +                  // Имя типа заказа
                    "[Perc] REAL NOT NULL);";// +                      // Процент
                    //"[Comment] NVARCHAR(250) NOT NULL);";            // Коментарий

    public readonly static string CreateOrderPoleStr =
                    "(" +
                    "[Id] BIGINT NOT NULL IDENTITY (1,1)," +           // uniqueidentifier
                    "[IdOrder] BIGINT NOT NULL," +                     // Id заказа
                    "[Number] INT NOT NULL," +                         // Порядковый номер поля

                    // Эти полe для того чтобы не удалять поля заказа в случае удаления или изменения типа заказа
                    "[NamePole] NVARCHAR(30) NOT NULL," +              // Наименование поля

                    "[Text] NVARCHAR(30) NOT NULL);";// +              // значение поля

    public readonly static string CreateTypeOrderPoleStr =
                    "(" +
                    "[Id] BIGINT NOT NULL IDENTITY (1,1)," +           // uniqueidentifier
                    "[IdTypeOrder] BIGINT NOT NULL," +                 // Id заказа
                    "[Number] INT NOT NULL," +                         // Порядковый номер поля
                    "[NamePole] NVARCHAR(30) NOT NULL);";              // Наименование поля
    }

    // Класс Заказа
    public class Order
    {
        public static string[] statusmas = new string[2] { "Не обработано", "Обработано" };
        public int Id;
        public string NuberDockument;
        public decimal Summ;
        int _Status;
        public int Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        public string StatusStr
        {
            get
            {
                return statusmas[_Status];
            }
        }
        public string NameTypeOrder;
        public decimal Percent;
        public List<OrderPole> ListPole;

        public Order(int id, string nuberdockument, decimal summ, int status, string nametypeorder, decimal percent)
        {
            Id = id;
            NuberDockument = nuberdockument;
            Summ = summ;
            Status = status;
            ListPole = new List<OrderPole>();
            NameTypeOrder = nametypeorder;
            Percent = percent;
        }
        public Order()
        {
            Id = -1;
            NuberDockument = "";
            Summ = 0;
            Status = 0;
            ListPole = new List<OrderPole>();
            NameTypeOrder = "";
            Percent = 0;
        }
        public void SetOrder(int id, string nuberdockument, decimal summ, int status, string nametypeorder, decimal percent)
        {
            Id = id;
            NuberDockument = nuberdockument;
            Summ = summ;
            Status = status;
            NameTypeOrder = nametypeorder;
            Percent = percent;
        }

    }

    // Класс дополнительных полей Заказа
    public class OrderPole
    {
        public int Id;
        public int IdOrder;
        public int Number;
        public string NamePole;
        public string Text;
        public OrderPole(int id, int idorder, int number, string namePole, string text)
        {
            SetOrderPole(id, idorder, number, namePole, text);
        }
        public OrderPole()
        {
            Id = -1;
            IdOrder = -1;
            Number = -1;
            NamePole = "";
            Text = "";
        }
        public void SetOrderPole(int id, int idorder, int number, string namePole, string text)
        {
            Id = id;
            IdOrder = idorder;
            Number = number;
            NamePole = namePole;
            Text = text;

        }

    }

    // Класс Типов Заказа
    public class TypeOrder
    {
        public int Id;
        public string Name;
        public decimal Percent;
        public List<TypeOrderPole> ListPole;
        public TypeOrder(int id, string name, decimal percent)
        {
            Id = id;
            Name=name;
            Percent = percent;
            ListPole = new List<TypeOrderPole>();
        }
        public TypeOrder()
        {
            Id = -1;
            Name = "";
            Percent = 0;
            ListPole = new List<TypeOrderPole>();
        }
        public void SetTypeOrder(int id, string name, decimal percent)
        {
            Id = id;
            Name = name;
            Percent = percent;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    // Класс Полей Типов Заказа
    public class TypeOrderPole
    {
        public int Id;
        public int IdTypeOrder;
        public int Number;
        public string NamePole;
        public TypeOrderPole(int id, int idtypeorder, int number, string namepole)
        {
            SetTypeOrderPole(id, idtypeorder, number, namepole);
        }
        public TypeOrderPole()
        {
            Id = -1;
            IdTypeOrder = -1;
            Number = -1;
            NamePole = "";
        }
        public void SetTypeOrderPole(int id, int idtypeorder, int number, string namepole)
        {
            Id = id;
            IdTypeOrder = idtypeorder;
            Number = number;
            NamePole = namepole;

        }
    }


    // Класс работающий с базо данных SQL CE
    public class ClassBase
    {

        public string FileName = "";
        public SqlCeCommand DBase = null;
        public bool IsOpen = false;

        string connString
        {
            get;
            set;
        }        

        public ClassBase(string filename, string endconnString)      
        {
            FileName = filename;
            connString = "Data Source=" + FileName + "; " + endconnString;
        }

        public bool Create()
        {
            bool ret = true;
            // Проверяем если файла нет в наличии то создаем базу данных
            if (!File.Exists(FileName))
            {
                //Создание файла базы данных !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                SqlCeEngine engine = new SqlCeEngine();
                
                //engine.

                try
                {
                    engine.LocalConnectionString = connString;        
                    engine.CreateDatabase();
                }
                catch
                {
                    ret = false;
                }
                finally
                {
                    if (engine != null) { engine.Dispose(); }
                }
            }
            return ret;
        }


        public bool Open()
        {
            // Открытие файла базы данных
            bool ret = false;
            if (File.Exists(FileName))
            {
                try
                {
                    DBase = new SqlCeCommand();
                    DBase.Connection = new SqlCeConnection(connString);
                    DBase.Connection.Open();
                    
                    IsOpen = true;
                    ret = true;
                }
                catch(Exception e) 
                { 
                    MessageBox.Show(e.Message, "Внимание!"); 
                }
            }
            else 
            { 
                MessageBox.Show("Файла нет в наличии!", "Внимание!"); 
            }
            return ret;
        }

        public void Close()
        {
            // Закрытие файла базы данных
            try
            {
                DBase.Connection.Close();
            }
            catch (Exception e) 
            { 
                MessageBox.Show(e.Message, "Внимание!"); 
            }
            IsOpen = false;
        }

        public bool TestTable(string TableName)         
            // проверить существование таблиц
        {
            // Проверка наличия таблицы в файле базы данных
            bool ret = false;
            try
            {
                DBase.CommandText = "Select * From " + TableName + " Where Id=0;";
                using (SqlCeDataReader rdr = DBase.ExecuteReader())
                {
                    rdr.Close();
                    ret = true;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("Тест - " + TableName +" - "+ e.Message, "Внимание!");
            }
            return ret;
        }

        public bool CreateTable(string Table) 
        {
            //Создание талиц базы данных
            bool ret = false;
            try                                                 
            {
                DBase.CommandText = Table;
                DBase.ExecuteNonQuery();
                ret = true;
            }
            catch (Exception e) 
            {
                MessageBox.Show(e.Message, "Внимание!"); 
            }
            return ret;
        }

        public bool CreateIndex(string TableName, string Pole, int NumberIndex)
        {
            //Создание индексов таблиц
            bool ret = false;
            try                                                 
            {
                DBase.CommandText = "CREATE INDEX [Idx_" + TableName + "_" + NumberIndex.ToString() + "] ON [" + TableName + "] ([" + Pole + "] ASC);";
                DBase.ExecuteNonQuery();
                ret = true;
            }
            catch(Exception e) 
            { 
                MessageBox.Show(e.Message, "Внимание!"); 
            }
            return ret;
        }

        public bool DelTable(string TableName)
        {
            // Удаление таблмц ;
            bool ret = false;
            try
            {
                DBase.CommandText = "DROP TABLE " + TableName + "; \r\n";
                int i = DBase.ExecuteNonQuery();
                ret = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }
            return ret;
        }
    }

    // Базовый класс для таблиц базы данных
    public class ClassTable
    {
        public ClassBase classBase = null;
        public SqlCeCommand DBase;
        public string TableName = "";
        string createTableStr = "";
        public string CreateTableStr
        {
            get { return "Create Table " + TableName +" "+ createTableStr; }
            set { createTableStr = value; }
        }

        public ClassTable(ClassBase classbase, string tablename, string createtablestr)
        {
            classBase = classbase;
            DBase = classBase.DBase;
            TableName = tablename.Trim();
            CreateTableStr = createtablestr;
            Param();
        }

        public virtual void Param()
        { 
        }

        public virtual bool CreateTable()
        {
            bool ret = false;
            if (classBase.CreateTable(CreateTableStr))
            {
                ret = CreateIndex();
            }
            return ret;
        }

        public virtual bool CreateIndex()
        {
            return true;
        }

    }

    // Класс таблицы Заказов
    public class ClassOrder : ClassTable
    {
        public DataSet dataSet = null;
        SqlCeDataAdapter Adapter = null;
        SqlCeCommandBuilder commandBuilder = null;

        public ClassOrder(ClassBase classbase)
            : base(classbase, "TableOrder", CreateStr.CreateOrderstr)
        {

        }

        public override void Param()
        {
            DBase.Parameters.Add("IdOrder", SqlDbType.Int);
            DBase.Parameters.Add("NDockOrder", SqlDbType.NVarChar, 20);
            DBase.Parameters.Add("SummOrder", SqlDbType.Real);
            DBase.Parameters.Add("StatusOrder", SqlDbType.Int);

            DBase.Parameters.Add("NameTypeInOrder", SqlDbType.NVarChar, 30);
            DBase.Parameters.Add("PercOrder", SqlDbType.Real);
        }

        public override bool CreateIndex()
        {
            bool ret = true;//false;
            ret = classBase.CreateIndex(TableName, "Id", 0);
            ret = classBase.CreateIndex(TableName, "NDock", 1);
            return ret;
        }

        public void Load()
        {
            if (Adapter == null)
            {

                string sql = "SELECT * FROM " + TableName + " ORDER BY Id;";
                Adapter = new SqlCeDataAdapter(sql, DBase.Connection);
                dataSet = new DataSet();

                // Для commandBuilder необходим хотябы один первичный ключ
                // столбец первичного ключа или столбец с атрибутом UNIQUE

               commandBuilder = new SqlCeCommandBuilder();                
               commandBuilder.DataAdapter = Adapter;

                //Adapter.InsertCommand
                    // Adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                    // Adapter.InsertCommand = commandBuilder.GetInsertCommand();
                    // Adapter.DeleteCommand = commandBuilder.GetDeleteCommand();

               //DbDataAdapter.OnRowUpdating 
               //RowUpdatingEventArgs value; //value.Command.Transaction
                 //DbDataAdapter.OnRowUpdated 
                 //RowUpdatedEventArgs e; //e.Command.Transaction
              
                   //StatementType.Insert // Тип выполненной операции
                 
                 //Clipboard.SetText("111111"); 

                // Вот так правильно
                //Clipboard.SetDataObject(new DataObject("Текст, который мы хотим скопировать в буфер обмена."));

                //MessageBox.Show(commandBuilder.GetUpdateCommand().CommandText);
                //MessageBox.Show(commandBuilder.GetInsertCommand().CommandText);
                //MessageBox.Show(commandBuilder.GetDeleteCommand().CommandText);

                /*
                SqlCeCommand upCmd = new SqlCeCommand(
                "update " + TableName + " set Summ=@Summ, Status=@Status where Id=@Id", DBase.Connection);
                upCmd.Parameters.Add("@Id", SqlDbType.Int, 4, "Id");
                upCmd.Parameters.Add("@NDock", SqlDbType.NVarChar, 20, "NDock");
                upCmd.Parameters.Add("@Summ", SqlDbType.Real, 10, "Summ");
                upCmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
                upCmd.Parameters.Add("@NameType", SqlDbType.NVarChar, 30, "NameType");
                upCmd.Parameters.Add("@Perc", SqlDbType.Real, 10, "Perc");
                
                Adapter.UpdateCommand = upCmd;
                 * 
                 * Если задавать команды без commandBuilder то можно то будет быстрее
                 * Можно сначала подключить commandBuilder скопировать строки для таблицы затем его отключить
                 * 
                 * Adapter.UpdateCommand
                 * UPDATE [TableOrder] SET [NDock] = @p1, [Summ] = @p2, [Status] = @p3, [NameType] = @p4, [Perc] = @p5 
                 *        WHERE (([Id] = @p6) AND ([NDock] = @p7) AND ([Summ] = @p8) AND ([Status] = @p9) AND ([NameType] = @p10) AND ([Perc] = @p11));
                 *        
                 * Adapter.InsertCommand
                 * INSERT INTO [TableOrder] ([NDock], [Summ], [Status], [NameType], [Perc]) VALUES (@p1, @p2, @p3, @p4, @p5)
                 * 
                 * Adapter.DeleteCommand
                 * DELETE FROM [TableOrder] 
                 *        WHERE (([Id] = @p1) AND ([NDock] = @p2) AND ([Summ] = @p3) AND ([Status] = @p4) AND ([NameType] = @p5) AND ([Perc] = @p6))
                */

            }
            else 
            { 
                dataSet.Clear(); 
            }
            Adapter.Fill(dataSet);
        }

       public void RefreshDS()
        {          
            Adapter.Update(dataSet);
        }
    }

    // Класс таблицы полей Заказа
    public class ClassOrderPole : ClassTable
    {

        public ClassOrderPole(ClassBase classbase)
            : base(classbase, "OrderPole", CreateStr.CreateOrderPoleStr)
        {

        }

        public override void Param()
        {
            DBase.Parameters.Add("IdOrderPole", SqlDbType.Int);
            DBase.Parameters.Add("IdOrderOrderPole", SqlDbType.Int);
            DBase.Parameters.Add("NumberOrderPole", SqlDbType.Int);
            DBase.Parameters.Add("NamePoleOrderPole", SqlDbType.NVarChar, 30);
            DBase.Parameters.Add("TextPoleOrderPole", SqlDbType.NVarChar, 30);
        }

        public override bool CreateIndex()
        {
            bool ret = false;
            ret = classBase.CreateIndex(TableName, "Id", 0);
            ret = classBase.CreateIndex(TableName, "IdOrder", 1);
            return ret;
        }

        public bool Load(Order objorder)
        {
            bool ret = false;
            DBase.CommandText = "SELECT * FROM " + TableName + " where IdOrder=@IdOrderOrderPole ORDER BY Number ";
            DBase.Parameters["IdOrderOrderPole"].Value = objorder.Id;
            try
            {
                SqlCeDataReader rdr = DBase.ExecuteReader();
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        objorder.ListPole.Add(new OrderPole(Convert.ToInt32(rdr["Id"].ToString()),
                                                           Convert.ToInt32(rdr["IdOrder"].ToString()),
                                                                Convert.ToInt32(rdr["Number"].ToString()),
                                                                              rdr["NamePole"].ToString(),
                                                                              rdr["Text"].ToString()));
                    }
                }
                rdr.Close();
                ret = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }


            return ret;
        }
    }

    // Класс таблицы Типов Заказа
    public class ClassTypeOrder : ClassTable
    {
        public DataSet dataSet=null;

       public ClassTypeOrder(ClassBase classbase)
            : base(classbase, "TypeOrder",CreateStr.CreateTypeOrderStr )
        { 

        }

        public override void Param()
        {
            DBase.Parameters.Add("IdType", SqlDbType.Int);
            DBase.Parameters.Add("NameType", SqlDbType.NVarChar, 30);
            DBase.Parameters.Add("PercentType", SqlDbType.Real);
        }

        public override bool CreateIndex()
        {
            bool ret = false;
            ret = classBase.CreateIndex(TableName, "Id", 0);
            return ret;
        }

       public void Load()
        {
            string sql = "SELECT * FROM " + TableName+" ORDER BY Id";
            SqlCeDataAdapter Adapter = new SqlCeDataAdapter(sql, DBase.Connection);
            dataSet = new DataSet();
            Adapter.Fill(dataSet);
        }
    }

    // Класс таблицы полей Типов Заказов
    public class ClassTypeOrderPole : ClassTable
    {
        public ClassTypeOrderPole(ClassBase classbase)
            : base(classbase, "TypeOrderPole", CreateStr.CreateTypeOrderPoleStr)
        {
        }

        public override void Param()
        {
            DBase.Parameters.Add("IdTypeOrderPole", SqlDbType.Int);
            DBase.Parameters.Add("IdTypeOrder", SqlDbType.Int);
            DBase.Parameters.Add("NumberTypeOrderPole", SqlDbType.Int);
            DBase.Parameters.Add("NamePoleTypeOrderPole", SqlDbType.NVarChar, 30);
        }

        public override bool CreateIndex()
        {
            bool ret = false;
            ret = classBase.CreateIndex(TableName, "Id", 0);
            classBase.CreateIndex(TableName, "IdTypeOrder", 1);
            return ret;
        }

        public bool Load(TypeOrder objtypeorder)
        {
            bool ret = false;
            DBase.CommandText = "SELECT * FROM " + TableName + " where IdTypeOrder=@IdTypeOrder ORDER BY Number ";
            DBase.Parameters["IdTypeOrder"].Value = objtypeorder.Id;
            try
            {
                SqlCeDataReader rdr = DBase.ExecuteReader();
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        objtypeorder.ListPole.Add(new TypeOrderPole(Convert.ToInt32(rdr["Id"].ToString()),
                                                           Convert.ToInt32(rdr["IdTypeOrder"].ToString()),
                                                                Convert.ToInt32(rdr["Number"].ToString()),
                                                                              rdr["NamePole"].ToString()));
                    }
                }
                rdr.Close();
                ret = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Внимание!");
            }


            return ret;
        }
    }


    // Создание или открытие базы и таблиц данных
    public class ClassOpenBase
    {
        public ClassTypeOrder classTypeOrder;
        public ClassTypeOrderPole classTypeOrderPole;
        public ClassOrder classOrder;
        public ClassOrderPole classOrderPole;
        ClassBase Base = null;
        public ClassOpenBase()
        {
            //  Создание или открытие базы и таблиц данных
            Base = new ClassBase("OrderBase.sdf", "Max Database Size = 4090; Mode = Read Write; Max Buffer Size = 10240;");
            if (Base.Create())
            {
                if (Base.Open())
                {
                    classTypeOrder = new ClassTypeOrder(Base);
                    if (Base.TestTable(classTypeOrder.TableName))
                    {
                        //tableOpen = true;
                    }
                    else
                    {
                        if (!classTypeOrder.CreateTable())
                        {
                            classTypeOrder = null;
                            MessageBox.Show("Таблица Типов Заказа не создана!", "Внимание!");
                        }
                        else { }// загружать нечего таблица создана но пустая
                    }

                    classTypeOrderPole = new ClassTypeOrderPole(Base);
                    if (Base.TestTable(classTypeOrderPole.TableName))
                    {
                        //tableOpen = true;
                    }
                    else
                    {
                        if (!classTypeOrderPole.CreateTable())
                        {
                            classTypeOrderPole = null;
                            MessageBox.Show("Таблица Полей Типов Заказа не создана!", "Внимание!");
                        }
                        else { }// загружать нечего таблица создана но пустая
                    }

                    classOrder = new ClassOrder(Base);
                    if (Base.TestTable(classOrder.TableName))
                    {
                        //tableOpen = true;
                    }
                    else
                    {
                        if (!classOrder.CreateTable())
                        {
                            classOrder = null;
                            MessageBox.Show("Таблица Заказа не создана!", "Внимание!");
                        }
                        else { }// загружать нечего таблица создана но пустая
                    }

                    classOrderPole = new ClassOrderPole(Base);
                    if (Base.TestTable(classOrderPole.TableName))
                    {
                        //tableOpen = true;
                    }
                    else
                    {
                        if (!classOrderPole.CreateTable())
                        {
                            classOrderPole = null;
                            MessageBox.Show("Таблица Полей Заказа не создана!", "Внимание!");
                        }
                        else { }// загружать нечего таблица создана но пустая
                    }
                }
                else { MessageBox.Show("Ошибка открытия базы!", "Внимание!"); }
            }
            else { MessageBox.Show("Ошибка создания базы!", "Внимание!"); } 

        }

    }


    // Добавление записи в Таблицу Тип Заказа
    class InsertTypeOrder
    {
        ClassTable typeorder;
        ClassTable pole;


        public InsertTypeOrder(ClassTable t1, ClassTable t2)
        {
            typeorder = t1;
            pole = t2;
        }

        public int Action(TypeOrder objtypeorder)
        {            
            int ret=-1;

            SqlCeTransaction tx = typeorder.DBase.Connection.BeginTransaction();
            typeorder.DBase.Transaction = tx;
            try
            {   //  Добавление нового типа заказа
                //  Добавляем запись типа заказа
                typeorder.DBase.CommandText = "INSERT INTO " + typeorder.TableName + " (Name, Perc) " +
                                       " VALUES (@NameType,@PercentType);\r\n";
                typeorder.DBase.Parameters["NameType"].Value = objtypeorder.Name;
                typeorder.DBase.Parameters["PercentType"].Value = objtypeorder.Percent;

                int n = typeorder.DBase.ExecuteNonQuery();
                typeorder.DBase.CommandText = "SELECT @@IDENTITY;";
                ret = Convert.ToInt32(typeorder.DBase.ExecuteScalar());
                objtypeorder.Id = ret;

                //  Добавление полей типа заказа
                for (int i = 0; i < objtypeorder.ListPole.Count; i++)
                {
                    objtypeorder.ListPole[i].IdTypeOrder = ret;
                    pole.DBase.CommandText = "INSERT INTO " + pole.TableName + " (IdTypeOrder,Number,NamePole)" +
                                           " VALUES (@IdTypeOrder,@NumberTypeOrderPole,@NamePoleTypeOrderPole);\r\n";

                    pole.DBase.Parameters["IdTypeOrder"].Value = objtypeorder.ListPole[i].IdTypeOrder;
                    pole.DBase.Parameters["NumberTypeOrderPole"].Value = objtypeorder.ListPole[i].Number;
                    pole.DBase.Parameters["NamePoleTypeOrderPole"].Value = objtypeorder.ListPole[i].NamePole;
                    n = pole.DBase.ExecuteNonQuery();
                    pole.DBase.CommandText = "SELECT @@IDENTITY;";
                    int r = Convert.ToInt32(pole.DBase.ExecuteScalar());
                    objtypeorder.ListPole[i].Id = r;
                }
                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                MessageBox.Show(e.Message, "Внимание!");
            }
            finally
            {
                typeorder.DBase.Transaction = null;
            }
            return ret;
        }

    }

    // Удаление Типа Заказа
    class DeleteTypeOrder
    {
        ClassTable typeorder;
        ClassTable pole;

        public DeleteTypeOrder(ClassTable t1, ClassTable t2)
        {
            typeorder = t1;
            pole = t2;
        }
        public bool Action(int id)
        {
            bool ret = false;

            SqlCeTransaction tx = typeorder.DBase.Connection.BeginTransaction();
            typeorder.DBase.Transaction = tx;
            try
            {
                // Удаляем все поля
                pole.DBase.CommandText = "DELETE " + pole.TableName + " Where IdTypeOrder=@IdTypeOrder;\r\n";
                pole.DBase.Parameters["IdTypeOrder"].Value = id;
                int i = pole.DBase.ExecuteNonQuery();

                // Удаляем запись типа заказа
                typeorder.DBase.CommandText = "DELETE " + typeorder.TableName + " Where Id=@IdType;\r\n";
                typeorder.DBase.Parameters["IdType"].Value = id;
                i = typeorder.DBase.ExecuteNonQuery();

                ret = true;

                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                MessageBox.Show(e.Message, "Внимание!");
            }
            finally
            {
                typeorder.DBase.Transaction = null;
            }

            return ret;
        }
    }

    // Изменение типа заказа
    class UpdateTypeOrder
    {
        ClassTable typeorder;
        ClassTable pole;

        public UpdateTypeOrder(ClassTable t1, ClassTable t2)
        {
            typeorder = t1;
            pole = t2;
        }
        public bool Action(TypeOrder objtypeorder)
        {
            bool ret = false;
            SqlCeTransaction tx = typeorder.DBase.Connection.BeginTransaction();
            typeorder.DBase.Transaction = tx;
            try
            {
                // Изменить запись Типа Заказа                                         
                typeorder.DBase.CommandText = "UPDATE " + typeorder.TableName + " Set Name=@NameType , Perc=@PercentType " +
                                                                                                      " where Id=@IdType; \r\n";
                typeorder.DBase.Parameters["IdType"].Value = objtypeorder.Id;
                typeorder.DBase.Parameters["NameType"].Value = objtypeorder.Name;
                typeorder.DBase.Parameters["PercentType"].Value = objtypeorder.Percent;
                int n = typeorder.DBase.ExecuteNonQuery();

                // Удалить все старые поля 
                pole.DBase.CommandText = "DELETE " + pole.TableName + " Where IdTypeOrder=@IdTypeOrder;\r\n";
                pole.DBase.Parameters["IdTypeOrder"].Value = objtypeorder.Id;
                n = pole.DBase.ExecuteNonQuery();
                
                // Сохранить новые поля
                for (int i = 0; i < objtypeorder.ListPole.Count; i++)
                {
                    objtypeorder.ListPole[i].IdTypeOrder = objtypeorder.Id;
                    pole.DBase.CommandText = "INSERT INTO " + pole.TableName + " (IdTypeOrder,Number,NamePole)" +
                                           " VALUES (@IdTypeOrder,@NumberTypeOrderPole,@NamePoleTypeOrderPole);\r\n";

                    pole.DBase.Parameters["IdTypeOrder"].Value = objtypeorder.ListPole[i].IdTypeOrder;
                    pole.DBase.Parameters["NumberTypeOrderPole"].Value = objtypeorder.ListPole[i].Number;
                    pole.DBase.Parameters["NamePoleTypeOrderPole"].Value = objtypeorder.ListPole[i].NamePole;
                    n = pole.DBase.ExecuteNonQuery();
                    pole.DBase.CommandText = "SELECT @@IDENTITY;";
                    int r = Convert.ToInt32(pole.DBase.ExecuteScalar());
                    objtypeorder.ListPole[i].Id = r;
                }

                ret = true;
                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                MessageBox.Show(e.Message, "Внимание!");
            }
            finally
            {
                typeorder.DBase.Transaction = null;
            }
            return ret;
        }
    }

    // Добавление записи в Таблицу Заказов
    class InsertOrder
    {
        ClassTable order;
        ClassTable pole;


        public InsertOrder(ClassTable t1, ClassTable t2)
        {
            order = t1;
            pole = t2;
        }

        public int Action(Order objorder)
        {
            int ret = -1;

            SqlCeTransaction tx = order.DBase.Connection.BeginTransaction();
            order.DBase.Transaction = tx;
            try
            {   //  Добавление нового заказа
                //  Добавляем запись заказа

                order.DBase.CommandText = "INSERT INTO " + order.TableName + " (NDock,Summ,Status,NameType, Perc) " +
                                          " VALUES (@NDockOrder,@SummOrder,@StatusOrder,@NameTypeInOrder,@PercOrder);\r\n";
                order.DBase.Parameters["NDockOrder"].Value = objorder.NuberDockument;
                order.DBase.Parameters["SummOrder"].Value = objorder.Summ;
                order.DBase.Parameters["StatusOrder"].Value = objorder.Status;
                order.DBase.Parameters["NameTypeInOrder"].Value = objorder.NameTypeOrder;
                order.DBase.Parameters["PercOrder"].Value = objorder.Percent;

                int n = order.DBase.ExecuteNonQuery();
                order.DBase.CommandText = "SELECT @@IDENTITY;";
                ret = Convert.ToInt32(order.DBase.ExecuteScalar());
                objorder.Id = ret;

                //  Добавление полей заказа
                for (int i = 0; i < objorder.ListPole.Count; i++)
                {
                    objorder.ListPole[i].IdOrder = ret;
                    pole.DBase.CommandText = "INSERT INTO " + pole.TableName + " (IdOrder,Number,NamePole,Text)" +
                                             " VALUES (@IdOrderOrderPole,@NumberOrderPole,@NamePoleOrderPole,@TextPoleOrderPole);\r\n";

                    pole.DBase.Parameters["IdOrderOrderPole"].Value = objorder.ListPole[i].IdOrder;
                    pole.DBase.Parameters["NumberOrderPole"].Value = objorder.ListPole[i].Number;
                    pole.DBase.Parameters["NamePoleOrderPole"].Value = objorder.ListPole[i].NamePole;
                    pole.DBase.Parameters["TextPoleOrderPole"].Value = objorder.ListPole[i].Text;
                    n = pole.DBase.ExecuteNonQuery();
                    pole.DBase.CommandText = "SELECT @@IDENTITY;";
                    int r = Convert.ToInt32(pole.DBase.ExecuteScalar());
                    objorder.ListPole[i].Id = r;
                }
                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                MessageBox.Show(e.Message, "Внимание!");
            }
            finally
            {
                order.DBase.Transaction = null;
            }
            return ret;
        }

    }

    // Удаление записи из талицы Заказов
    class DeleteOrder
    {
        ClassTable order;
        ClassTable pole;

        public DeleteOrder(ClassTable t1, ClassTable t2)
        {
            order = t1;
            pole = t2;
        }
        public bool Action(int id)
        {
            bool ret = false;

            SqlCeTransaction tx = order.DBase.Connection.BeginTransaction();
            order.DBase.Transaction = tx;
            try
            {
                // Удаляем все поля
                pole.DBase.CommandText = "DELETE " + pole.TableName + " Where IdOrder=@IdOrderOrderPole;\r\n";
                pole.DBase.Parameters["IdOrderOrderPole"].Value = id;
                int i = pole.DBase.ExecuteNonQuery();

                // Удаляем запись типа заказа
                order.DBase.CommandText = "DELETE " + order.TableName + " Where Id=@IdOrder;\r\n";
                order.DBase.Parameters["IdOrder"].Value = id;
                i = order.DBase.ExecuteNonQuery();

                ret = true;

                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                MessageBox.Show(e.Message, "Внимание!");
            }
            finally
            {
                order.DBase.Transaction = null;
            }

            return ret;
        }
    }

    // Изменение записи в таблице Заказов
    class UpdateOrder
    {
        ClassTable order;
        ClassTable pole;

        public UpdateOrder(ClassTable t1, ClassTable t2)
        {
            order = t1;
            pole = t2;
        }
        public bool Action(Order objorder)
        {
            bool ret = false;
            SqlCeTransaction tx = order.DBase.Connection.BeginTransaction(); //Добавлять уровень блокировок и транзакции при чтении
            order.DBase.Transaction = tx;

            //order.DBase.Transaction.
            ///IsolationLevel.
            try
            {                                          
                // Изменить запись Заказа                                         
                order.DBase.CommandText = "UPDATE " + order.TableName + " Set NDock=@NDockOrder, Summ=@SummOrder, Status=@StatusOrder," +
                                                                        " NameType=@NameTypeInOrder, Perc=@PercOrder " +
                                                                        " where Id=@IdOrder; \r\n";
                order.DBase.Parameters["IdOrder"].Value = objorder.Id;
                order.DBase.Parameters["NDockOrder"].Value = objorder.NuberDockument;
                order.DBase.Parameters["SummOrder"].Value = objorder.Summ;
                order.DBase.Parameters["StatusOrder"].Value = objorder.Status;
                order.DBase.Parameters["NameTypeInOrder"].Value = objorder.NameTypeOrder;
                order.DBase.Parameters["PercOrder"].Value = objorder.Percent;                

                int n = order.DBase.ExecuteNonQuery();

                // Удалить все старые поля 
                pole.DBase.CommandText = "DELETE " + pole.TableName + " Where IdOrder=@IdOrderOrderPole;\r\n";
                pole.DBase.Parameters["IdOrderOrderPole"].Value = objorder.Id;
                n = pole.DBase.ExecuteNonQuery();

                // Сохранить новые поля
                for (int i = 0; i < objorder.ListPole.Count; i++)
                {
                    objorder.ListPole[i].IdOrder = objorder.Id;
                    pole.DBase.CommandText = "INSERT INTO " + pole.TableName + " (IdOrder,Number,NamePole,Text)" +
                                             " VALUES (@IdOrderOrderPole,@NumberOrderPole,@NamePoleOrderPole,@TextPoleOrderPole);\r\n";

                    pole.DBase.Parameters["IdOrderOrderPole"].Value = objorder.ListPole[i].IdOrder;
                    pole.DBase.Parameters["NumberOrderPole"].Value = objorder.ListPole[i].Number;
                    pole.DBase.Parameters["NamePoleOrderPole"].Value = objorder.ListPole[i].NamePole;
                    pole.DBase.Parameters["TextPoleOrderPole"].Value = objorder.ListPole[i].Text;
                    n = pole.DBase.ExecuteNonQuery();
                    pole.DBase.CommandText = "SELECT @@IDENTITY;";
                    int r = Convert.ToInt32(pole.DBase.ExecuteScalar());
                    objorder.ListPole[i].Id = r;
                }

                ret = true;
                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                MessageBox.Show(e.Message, "Внимание!");
            }
            finally
            {
                order.DBase.Transaction = null;
            }
            return ret;
        }
    }
}
