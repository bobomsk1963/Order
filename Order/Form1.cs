using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data.SqlServerCe;

//             Здравствуте!
/*             
 *             В эотой программе реализована возможность добалять множество Типов Заказа
 *             с возможностью указывать свой процент и набор полей которые должны заполняться
 *             в Заказе. Созданные заказы можно редактировать и менять тип заказа.
 *             Программа специально реализована в одном окне с возможностью осуществлять навигацю по вкладкам PageControl.
 *             В базе данных уже введены Типы заказов с параметрами описанными в задаче.
 *             База данных выбрана для простоты под MSSQL CE 4.0.              
*/
//             Богоявленский Борис
//             адрес bobomsk1963@mail.ru 
//
namespace Order
{
    public partial class Form1 : Form
    {
        ClassOpenBase Base;

        CustomClass TypeOrderPoleProperties = new CustomClass();
        CustomClass OrderPoleProperties = new CustomClass();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            // Скрытие закладок которые будут использоваться для добавления или измениения 
            // записей в базы данных

            // Для таблицы Заказы
            tabControl2.TabPages.RemoveAt(1);
            // Для таблицы Типы заказа
            tabControl3.TabPages.RemoveAt(1);

            // К редакторам свойств подключаем классы их наполняющие
            propertyGrid2.SelectedObject = TypeOrderPoleProperties;
            propertyGrid1.SelectedObject = OrderPoleProperties;

            // Открытие или создание базы данных
            Base = new ClassOpenBase();

            // Загрузка данных в виртуальные таблицы
            UpdateTableTypeOrder();
            UpdateTableOrder();
        }

        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Возврат к просмотру таблицы Разновидности
            if ((tabControl3.SelectedIndex == 0)&&(tabControl3.TabPages.Count > 1))
                {
                    tabControl3.TabPages.RemoveAt(1);
                    TypeOrderPoleProperties.Clear();
                    propertyGrid2.Refresh();
                    button10.Tag = null;
                }            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Нажатие кнопки Добавить разновидность заказов

            // Изменение заголовка вкладки
            tabPage6.Text = "Добавление";
            // Отобразить вкладку
            tabControl3.TabPages.Add(tabPage6);
            tabControl3.SelectedIndex = 1;
            button10.Text = "Добавить";

            // Очистить данные
            textBox1.Text = "";
            textBox2.Text = "0";
            textBox3.Text = "";
            TypeOrderPoleProperties.Clear();

            //((CustomClass)propertyGrid2.SelectedObject).Clear();

            textBox1.Focus();
            button10.Tag = null;

        }

        private void button11_Click(object sender, EventArgs e)
        {
            // Нажатие на кнопку добавление Поля в таблицу
            if (textBox3.Text.Trim() != "")
            {
                CustomProperty myProp = new CustomProperty((TypeOrderPoleProperties.Count + 1).ToString(), textBox3.Text, false, true);
                TypeOrderPoleProperties.Add(myProp);
                propertyGrid2.Refresh();

                textBox3.Focus();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // Нажатие на кнопку добавление или изменение Типа Заказа

                // Проверка данных на возможность добавления
                if (textBox1.Text.Trim() == "")
                {
                    MessageBox.Show("Поле Наименование пустое его необходимо заполнить!", "Внимание!");
                    return;
                }

                //TypeOrderPoleProperties[
                // Проверка наличия повторений в наименованиях полей
                if (TypeOrderPoleProperties.Count == 0)
                {
                    MessageBox.Show("Должно быть хотябы одно наименование поля!", "Внимание!");
                    return;
                }

                for (int i = 0; i < TypeOrderPoleProperties.Count; i++)
                {
                    if (((string)TypeOrderPoleProperties[i].Value).Trim() == "")
                    {
                        MessageBox.Show("Все имена полей не должны быть пустыми!", "Внимание!");
                        return;
                    }

                    for (int j = i + 1; j < TypeOrderPoleProperties.Count; j++)
                    {
                        if (((string)TypeOrderPoleProperties[i].Value).Trim() == ((string)TypeOrderPoleProperties[j].Value).Trim())
                        {
                            MessageBox.Show("Имена полей не должны совпадать!", "Внимание!");
                            return;
                        }
                    }

                }

                if (((Button)sender).Tag == null)
                {
                    // Добавление
                    // Проверка на совпадения в именах таблицы Типов Заказов 
                    string text = textBox1.Text.Trim();
                    for (int i = 0; i < Base.classTypeOrder.dataSet.Tables[0].Rows.Count; i++)
                    {
                        DataRow row = Base.classTypeOrder.dataSet.Tables[0].Rows[i];
                        if (row[1].ToString() == text)
                        {
                            MessageBox.Show("Обнаружено повторение имени Типа Заказа!", "Внимание!");
                            return;
                        }
                    }

                    addTipeOrder();

                }
                else
                {
                    // Изменение
                    // Проверка на совпадения в именах таблицы Типов Заказов 
                    string text = textBox1.Text.Trim();
                    for (int i = 0; i < Base.classTypeOrder.dataSet.Tables[0].Rows.Count; i++)
                    {
                        DataRow row = Base.classTypeOrder.dataSet.Tables[0].Rows[i];
                        if ((Int32.Parse(row[0].ToString()) != ((TypeOrder)button10.Tag).Id) && (row[1].ToString() == text))
                        {
                            MessageBox.Show("Обнаружено повторение имени Типа Заказа!", "Внимание!");
                            return;
                        }
                    }

                    updateTipeOrder();
                }

            Base.classTypeOrder.Load();
            listView2.VirtualListSize = Base.classTypeOrder.dataSet.Tables[0].Rows.Count;
            //listView2.VirtualMode = true;
            listView2.Refresh();
            tabControl3.SelectedIndex = 0;
            
        }

        void updateTipeOrder()
        {
            // Изменение Типа Заказа
            Decimal d = 0;
            try
            {
                d = Decimal.Parse(textBox2.Text);
            }
            catch { }

            TypeOrder objtypeorder = (TypeOrder)button10.Tag;
            objtypeorder.Name = textBox1.Text.Trim();
            objtypeorder.Percent = d;
            objtypeorder.ListPole.Clear();            
            for (int i = 0; i < TypeOrderPoleProperties.Count; i++)
            {
                objtypeorder.ListPole.Add(new TypeOrderPole(-1, -1, i + 1, (string)TypeOrderPoleProperties[i].Value));
            }

            UpdateTypeOrder updateTypeOrder = new UpdateTypeOrder(Base.classTypeOrder, Base.classTypeOrderPole);
            bool b = updateTypeOrder.Action(objtypeorder);
        }

        void addTipeOrder()
        {
            //Добавление нового тип Заказа
            Decimal d=0;
            try
            {
                d = Decimal.Parse(textBox2.Text);
            }
            catch { }

            // Формируем структуру
            TypeOrder objtypeorder = new TypeOrder(-1, textBox1.Text.Trim(), d);//Convert.ToInt32(textBox2.Text));            
            for (int i=0;i<TypeOrderPoleProperties.Count;i++)
            {
                objtypeorder.ListPole.Add(new TypeOrderPole(-1, -1, i + 1, (string)TypeOrderPoleProperties[i].Value));
            }

            // Передаем структуру в добавление 
            InsertTypeOrder insertTypeOrder = new InsertTypeOrder(Base.classTypeOrder, Base.classTypeOrderPole);
            int n= insertTypeOrder.Action(objtypeorder);
           
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Событие происходящее при изменении номера отображаемой закладки 
            if (tabControl1.SelectedIndex == 1)
            {
                // Загрузка Типов Заказа
                UpdateTableTypeOrder();

                // Если начато добавление или изменение Заказа то отменить 
                if (tabControl2.SelectedIndex == 1)
                {
                    tabControl2.SelectedIndex = 0;
                }

            }
            else
            {
                UpdateTableOrder();

                // Если начато добавление или изменение Типа Заказа то отменить 
                if (tabControl3.SelectedIndex == 1)
                {
                    tabControl3.SelectedIndex = 0;
                }
            }
        }

        void UpdateTableTypeOrder()
        {
            Base.classTypeOrder.Load();
            listView2.VirtualListSize = Base.classTypeOrder.dataSet.Tables[0].Rows.Count;
            listView2.VirtualMode = true;
            listView2.Refresh();
        }

        private void listView2_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // Событие вывода в виртуальную таблицу
            int c = Base.classTypeOrder.dataSet.Tables[0].Rows.Count;
            if (((c > 0) && (e.ItemIndex < c)) && (e.ItemIndex > -1))
            {
                DataRow row = Base.classTypeOrder.dataSet.Tables[0].Rows[e.ItemIndex];
                e.Item = new ListViewItem(new string[] { (e.ItemIndex + 1).ToString(),row[1].ToString() ,row[2].ToString() });                
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Нажатие кнопки отмена добавления нового Типа Заказа
            tabControl3.SelectedIndex = 0;
        }

        private void button6_Click(object sender, EventArgs e)
        {

            // Вывести данные

            if (listView2.SelectedIndices.Count > 0)
            {
                // Нажатие кнопки изменение в Таблице разновидностей заказов
                int n = listView2.SelectedIndices[0];

                // 
                DataRow row = Base.classTypeOrder.dataSet.Tables[0].Rows[n];
                TypeOrder objtypeorder = new TypeOrder(Int32.Parse(row[0].ToString()), row[1].ToString(), Decimal.Parse(row[2].ToString()));

                // Загрузить имена полей
                Base.classTypeOrderPole.Load(objtypeorder);

                // Изменение заголовка вкладки
                tabPage6.Text = "Изменение";
                button10.Text = "Изменить";

                // Помещаем объект который будет изменяться вот сюда
                button10.Tag = objtypeorder;

                textBox1.Text=objtypeorder.Name;
                textBox2.Text = objtypeorder.Percent.ToString();

                // Заполним таблицу полей
                TypeOrderPoleProperties.Clear();
                for (int i = 0; i < objtypeorder.ListPole.Count; i++)
                { 
                CustomProperty myProp = new CustomProperty(objtypeorder.ListPole[i].Number.ToString(), objtypeorder.ListPole[i].NamePole, false, true);
                TypeOrderPoleProperties.Add(myProp);                
                }
                propertyGrid2.Refresh();

                // Отобразить вкладку
                tabControl3.TabPages.Add(tabPage6);
                tabControl3.SelectedIndex = 1;
                
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // Кнопка удаления имени поля
            TypeOrderPoleProperties.Remove(propertyGrid2.SelectedGridItem.Label);
            for (int i = 0; i < TypeOrderPoleProperties.Count; i++)
            {
                TypeOrderPoleProperties[i].Name = (i + 1).ToString();
            }

            propertyGrid2.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Кнопка удаления Типа Заказа
            if (listView2.SelectedIndices.Count > 0)
            {
                // Нажатие кнопки изменение в Таблице разновидностей заказов
                int n = listView2.SelectedIndices[0];

                DataRow row = Base.classTypeOrder.dataSet.Tables[0].Rows[n];
                int id = Int32.Parse(row[0].ToString());

                DeleteTypeOrder deleteTypeOrder = new DeleteTypeOrder(Base.classTypeOrder, Base.classTypeOrderPole);
                bool b = deleteTypeOrder.Action(id);

            Base.classTypeOrder.Load();
            listView2.VirtualListSize = Base.classTypeOrder.dataSet.Tables[0].Rows.Count;
            listView2.Refresh();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Кнопка добавить заказ
            tabPage4.Text = "Добавление";
            button4.Text = "Добавить";


            Order order = null; //new Order();
            button4.Tag = order;

            OrderToScreen(null);

            LoadComboBox();            

            tabControl2.TabPages.Add(tabPage4);
            tabControl2.SelectedIndex = 1;
        }

        void OrderToScreen(Order order)
        { 
            // Заполнение экранной формв редактирования заказа
            // Сначала очистка
            textBox4.Text = "";
            textBox5.Text = "";
            //textBox6.Text = "";
            comboBox2.SelectedIndex = 0;
            textBox7.Text = "";
            textBox8.Text = "";
            OrderPoleProperties.Clear();

            if (order != null)
            {   // Если редактирование заказа то заполняем экранные формы
                textBox4.Text = order.NuberDockument;
                textBox5.Text = order.Summ.ToString();
                //textBox6.Text = order.Status.ToString();
                comboBox2.SelectedIndex = order.Status;
                textBox7.Text = order.NameTypeOrder;
                textBox8.Text = order.Percent.ToString();
                for (int i = 0; i < order.ListPole.Count; i++)
                {
                    CustomProperty myProp = new CustomProperty(order.ListPole[i].NamePole, order.ListPole[i].Text, false, true);
                    OrderPoleProperties.Add(myProp);
                }
                
            }

            propertyGrid1.Refresh();
            // Провермть если статус=1 то заблокировать изменение типа заказа и изменение суммы
            groupBox2.Visible = (order == null) || (order.Status == 0);
            textBox5.ReadOnly = (order != null) && (order.Status == 1);

        }

        void LoadComboBox()
        { // Загрузка комбобокса для выбора типа Закза

            // Обновить дата сет
            UpdateTableTypeOrder();

            // Загрузить 
            comboBox1.Items.Clear();
            for (int i = 0; i < Base.classTypeOrder.dataSet.Tables[0].Rows.Count; i++)
            { 
                DataRow row = Base.classTypeOrder.dataSet.Tables[0].Rows[i];
                int id = Int32.Parse(row[0].ToString());
                TypeOrder typeOrder = new TypeOrder(id, row[1].ToString(), Decimal.Parse(row[2].ToString()));

                Base.classTypeOrderPole.Load(typeOrder);
                comboBox1.Items.Add(typeOrder);
                // Загружать списки полей
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Событие происходящее при изменении номера отображаемой закладки 
            if (tabControl2.SelectedIndex == 0)
            {
                tabControl2.TabPages.RemoveAt(1);
                OrderPoleProperties.Clear();
                button4.Tag = null;
                propertyGrid1.Refresh();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Выбор пункта в комбобокс
            listBox1.Items.Clear();
            for (int i = 0; i < ((TypeOrder)((ComboBox)sender).SelectedItem).ListPole.Count; i++)
            {
                listBox1.Items.Add(((TypeOrder)((ComboBox)sender).SelectedItem).ListPole[i].NamePole);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // Выбор типа заказа 

            //Выгрузка типа заказа в свойства
            OrderPoleProperties.Clear();
            for (int i = 0; i < ((TypeOrder)comboBox1.SelectedItem).ListPole.Count; i++)
            {
                CustomProperty myProp = new CustomProperty(((TypeOrder)comboBox1.SelectedItem).ListPole[i].NamePole, "", false, true);
                OrderPoleProperties.Add(myProp);                
            }
            propertyGrid1.Refresh();
            textBox7.Text = ((TypeOrder)comboBox1.SelectedItem).Name;
            textBox8.Text = ((TypeOrder)comboBox1.SelectedItem).Percent.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Добавление Заказа

            // Проверка на возможность добавления
            textBox4.Text=textBox4.Text.Trim();
            if (textBox4.Text == "")
            {
                MessageBox.Show("Поле Номер Заказа пустое его необходимо заполнить!", "Внимание!");
                return;
            }

            if (OrderPoleProperties.Count == 0)
            {
                MessageBox.Show("Тип заказа не выбран!", "Внимание!");
                return;
            }

            if (((Button)sender).Tag == null)
            {
                // Добавление
                // Проверка на совпадения номеров Заказов 
                string text = textBox4.Text.Trim();
                for (int i = 0; i < Base.classOrder.dataSet.Tables[0].Rows.Count; i++)
                {
                    DataRow row = Base.classOrder.dataSet.Tables[0].Rows[i];
                    if (row[1].ToString() == text)
                    {
                        MessageBox.Show("Обнаружено повторение Номера Заказа!", "Внимание!");
                        return;
                    }
                }


                // Формирование объекта для добавления и добавление
                addOrder();
            }
            else
            {
                // Изменение
                // Проверка на совпадения номеров Заказов  
                string text = textBox4.Text.Trim();
                for (int i = 0; i < Base.classOrder.dataSet.Tables[0].Rows.Count; i++)
                {
                    DataRow row = Base.classOrder.dataSet.Tables[0].Rows[i];
                    if ((Int32.Parse(row[0].ToString()) != ((Order)button4.Tag).Id) && (row[1].ToString() == text))
                    {
                        MessageBox.Show("Обнаружено повторение имени Типа Заказа!", "Внимание!");
                        return;
                    }
                }

                updateOrder();
            }
            
            UpdateTableOrder();
            tabControl2.SelectedIndex = 0;
        }

        void UpdateTableOrder()
        {   // Обновление DataSet талицы Заказов
            Base.classOrder.Load();
            listView1.VirtualListSize = Base.classOrder.dataSet.Tables[0].Rows.Count;
            listView1.VirtualMode = true;
            listView1.Refresh();
        }

        void addOrder()
        {
            //Добавление нового Заказа
            Decimal s = 0;
            try
            {
                s = Decimal.Parse(textBox5.Text);
            }
            catch { }
            Decimal p = 0;
            try
            {
                p = Decimal.Parse(textBox8.Text);
            }
            catch { }

            // Формируем структуру
            
            Order objorder = new Order(-1, textBox4.Text.Trim(), s,0,textBox7.Text,p);            
            for (int i = 0; i < OrderPoleProperties.Count; i++)
            {                                  
                objorder.ListPole.Add(new OrderPole(-1, -1, i + 1, (string)OrderPoleProperties[i].Name,(string)OrderPoleProperties[i].Value));
            }

            // Передаем структуру в добавление 
            InsertOrder insertOrder = new InsertOrder(Base.classOrder, Base.classOrderPole);
            int n = insertOrder.Action(objorder);
        }

        void updateOrder()
        // Изменение Заказа
        {
            Decimal s = 0;
            try
            {
                s = Decimal.Parse(textBox5.Text);
            }
            catch { }
            Decimal p = 0;
            try
            {
                p = Decimal.Parse(textBox8.Text);
            }
            catch { }

            Order objorder = (Order)button4.Tag;

            objorder.NuberDockument = textBox4.Text.Trim();
            objorder.Summ = s;
            //objorder.Status= // 
            objorder.NameTypeOrder = textBox7.Text;
            objorder.Percent = p;

            objorder.ListPole.Clear();
            for (int i = 0; i < OrderPoleProperties.Count; i++)
            {
                objorder.ListPole.Add(new OrderPole(-1, -1, i + 1, (string)OrderPoleProperties[i].Name, (string)OrderPoleProperties[i].Value));
            }

            
            UpdateOrder updateOrder = new UpdateOrder(Base.classOrder, Base.classOrderPole);
            bool b = updateOrder.Action(objorder);
             
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // Событие вывода в виртуальную таблицу Заказов
            int c = Base.classOrder.dataSet.Tables[0].Rows.Count;
            if (((c > 0) && (e.ItemIndex < c)) && (e.ItemIndex > -1))
            {
                DataRow row = Base.classOrder.dataSet.Tables[0].Rows[e.ItemIndex];
                e.Item = new ListViewItem(new string[] { (e.ItemIndex + 1).ToString(), row[1].ToString(), 
                                    row[2].ToString(), Order.statusmas[Int32.Parse(row[3].ToString())], row[4].ToString() });
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Изменение Заказа

            // Проверяем установлен ли курсор на элементе таблицы
            if (listView1.SelectedIndices.Count > 0)
            {
                // Нажатие кнопки изменение Заказа
                // Если установлен то считываем номер позиции курсора
                int n = listView1.SelectedIndices[0];

                // Считываем строку из dataSet
                DataRow row = Base.classOrder.dataSet.Tables[0].Rows[n];                
                // Заполняем Order
                Order order = new Order(Int32.Parse(row[0].ToString()), 
                                                              row[1].ToString(), 
                                                                        Decimal.Parse(row[2].ToString()),
                                                                                 Int32.Parse(row[3].ToString()),
                                                                                 row[4].ToString(), Decimal.Parse(row[5].ToString()));

                Base.classOrderPole.Load(order);

                // Помещаем объект на кнопку подтверждения внесения изменений после редактирования
                button4.Tag = order;

                // Заполняем экранную форму
                OrderToScreen(order);

                // Загрузка данных в форму выбор типа заказов
                LoadComboBox();

                tabPage4.Text = "Изменение";
                button4.Text = "Изменить";

                // Переходим на закладку редактирования
                tabControl2.TabPages.Add(tabPage4);                 
                tabControl2.SelectedIndex = 1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                // Нажатие кнопки удаление в Таблице Заказов
                int n = listView1.SelectedIndices[0];

                DataRow row = Base.classOrder.dataSet.Tables[0].Rows[n];
                int id = Int32.Parse(row[0].ToString());

                DeleteOrder deleteOrder = new DeleteOrder(Base.classOrder, Base.classOrderPole);
                bool b = deleteOrder.Action(id);

                Base.classOrder.Load();
                listView1.VirtualListSize = Base.classOrder.dataSet.Tables[0].Rows.Count;
                listView1.Refresh();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Нажатие кнопки отмена редактирования Заказа
            tabControl2.SelectedIndex = 0;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                // Нажатие кнопки Обработать Заказов
                int n = listView1.SelectedIndices[0];

                DataRow row = Base.classOrder.dataSet.Tables[0].Rows[n];
                if (Int32.Parse(row[3].ToString()) == 0)
                {
                    Decimal summ = 0;
                    try
                    {
                        summ = Decimal.Parse(row[2].ToString());
                    }
                    catch { }
                    Decimal proc = 0;
                    try
                    {
                        proc = Decimal.Parse(row[5].ToString());
                    }
                    catch { }
                    summ = (summ / 100) * (100 + proc);

                    row[2] = summ;
                    row[3] = 1;
                    //row.AcceptChanges();

                    
                    Base.classOrder.RefreshDS();

                    listView1.Refresh();

                }

            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                // Нажатие кнопки отмена Обработки Заказов
                int n = listView1.SelectedIndices[0];

                DataRow row = Base.classOrder.dataSet.Tables[0].Rows[n];
                if (Int32.Parse(row[3].ToString()) == 1)
                {
                    Decimal summ = 0;
                    try
                    {
                        summ = Decimal.Parse(row[2].ToString());
                    }
                    catch { }
                    Decimal proc = 0;
                    try
                    {
                        proc = Decimal.Parse(row[5].ToString());
                    }
                    catch { }

                    summ = (summ / (100 + proc))*100;

                    row[2] = summ;
                    row[3] = 0;

                    Base.classOrder.RefreshDS();
                    listView1.Refresh();

                }

            }
        }

    }
}
