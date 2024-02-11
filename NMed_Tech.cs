using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Threading;
using System.Data.OleDb;
using System.Drawing;
using System.Diagnostics;

namespace NMed_Tech{

    public partial class Diagnostic_Program : Form{     
 
        bool Fast; //быстрый вывод 
        bool SaveToDB; //сохранение в БД
        bool ProtectionOfButton=false; //переменная, отвечающая за защиту кнопки от многократных кликов
        bool MessageOn; //на экране сообщение
        int MessageAmount = 0; //защита от повторяющихся сообщений
         
        public Diagnostic_Program() {
            InitializeComponent();
            StreamReader FileSR = new StreamReader("theme.txt");
            int Theme = Int32.Parse(FileSR.ReadLine()); //считывание номера сохраненной цветовой темы из файла
            SelectTheme(Theme);//функция установки сохраненной темы при загрузке формы
            FileSR.Close();
            comboBox1.SelectedIndex = Theme; //установка переключателя темы в нужное положение 
            StreamReader FileSR1 = new StreamReader("fast.txt");
            Fast = bool.Parse(FileSR1.ReadLine()); //считывание из файла настройки включения/выключения быстрого вывода          
            FileSR1.Close();
            checkBox1.Checked = Fast; //установка нужного режима вывода текста
            StreamReader FileSR2 = new StreamReader("save_to_DB.txt");
            SaveToDB = bool.Parse(FileSR2.ReadLine()); //считывание из файла настройки записи данных в БД
            FileSR2.Close();
            checkBox2.Checked = SaveToDB;} //установка включения/выключ. сохранения в БД
        
        //Ф-Я НАЖАТИЯ НА КНОПКУ "ДИАГНОСТИКА"
        private void button1_Click(object sender, EventArgs e){

           //for (int i = 0; i < 200; i++) SymptomList.SetItemChecked(i, true); //тест для отладки: выбрать все симптомы

           //если хоть один симптом выбран и защиты нет:
            if (SymptomList.CheckedItems.Count != 0 && ProtectionOfButton==false){

                Main_Diagnostic_Function();

                ProtectionOfButton = true;//ставим защиту от кликов по кнопке диагностики   
        }}

        //ГЛАВНАЯ Ф-Я. АЛГОРИТМ СВЕРКИ СИМПТОМОВ И ПОСТАНОВКИ ДИАГНОЗА
        //************************************************************
        public void Main_Diagnostic_Function(){      
            //тест на время выполнения функции (начало)
            //Stopwatch stopWatch = new Stopwatch(); stopWatch.Start();

            print("\n\nПОИСК СООТВЕТСТВИЯ:\n\n"); scr();
            string Supposed_Diagnosis = "?";
            int n=0; //кол-во выбранных симптомов (кол-во эл-в массива)
            foreach (object itemChecked in SymptomList.CheckedItems) n++;
            string[] M1 = new string[n]; //массив М1 из выбранных симптомов
            int k=0;//счетчик эл-в массива
            foreach (object itemChecked in SymptomList.CheckedItems){ //заполнение массива
                M1[k]=itemChecked.ToString();
                k++; }

            string diagnosis;
            string[] Strings = System.IO.File.ReadAllLines(@".\Knowledge_Base\diagnosis-symptoms.txt", 
                Encoding.Default);//массив строк из БЗ          
            //массив, в который будут записаны коэффициенты соответствия каждому диагнозу:
            int[] MasOfMatch = new int[Strings.Length];
            int t = 0; //индекс массива MasOfMatch
            foreach (string str in Strings){ //перебор всех строк из БЗ                     
                string[] M2 = MassiveOfSymptoms(str); //массив М2 из симптомов в строке
                int Match = 0;//счетчик соответствия
                //сравниваем соответствие выбранных симптомов с симптомами в строке
                for (int i = 0; i < M2.Length; i++)
                { for (int j = 0; j < M1.Length; j++) if (M2[i] == M1[j]) Match++; } //счетчик соответствия          
                MasOfMatch[t] = Match;//каждому диагнозу присваивается подсчитанный коэффициент соответствия
                /////ПРОМЕЖУТОЧНЫЙ ВЫВОД//////
                if (MasOfMatch[t] != 0){
                    diagnosis = devide_diagnosis(str);//промежуточный вывод диагноза
                    print(diagnosis + " ");
                    //промежуточный вывод соответствия симптомов для каждого диагноза
                    print("[соответствие = " + MasOfMatch[t] + " из " + M2.Length + "]\n");
                    scr();}
                ///////
                t++; }
            print("\n"); scr();

            //поиск максимального коэффициента
            int max = MasOfMatch[0], maxIndex = 0;
            for (int i = 0; i < Strings.Length; i++){
                if (MasOfMatch[i] > max){ //если найден больший коэфф-т соответствия
                    max = MasOfMatch[i];
                    maxIndex = i; }}

            //массив, в котором будут храниться индексы повторяющихся макс.коэфф-в
            //то есть одинаково подходящих диагнозов
            int[] MasOfIndex = new int[MasOfMatch.Length];
            double[] MasOfConfidence = new double[MasOfMatch.Length];
            int p = 0;//счетчик предположительных диагнозов по одинаковым коэффициентам соответствия
            string[] Supposed_Diagnoses = new string[MasOfMatch.Length]; //массив предположительных диагнозов
            //print("индексы совпадающих max коэф-в:\n");//промежуточно
            for (int i = 0; i < MasOfMatch.Length; i++) {//поиск всех повторяющихся макс. коэффициентов
                if (MasOfMatch[i] == max){//если найден такой же коэфф-т соответствия
                    MasOfIndex[p] = i;
                    //print(MasOfIndex[p].ToString()+"\n");//промежуточный вывод индекса
                    diagnosis = devide_diagnosis(Strings[i]); //отделяем диагноз и сохраняем в переменную
                    print("ПРЕДПОЛОЖИТЕЛЬНЫЙ ДИАГНОЗ " + "№" + (p+1) + ": " + diagnosis + "\n"); //выводим итоговый диагноз
                    Supposed_Diagnoses[p] = diagnosis+"*"; //заносим диагноз в массив предположительных диагнозов
                    scr();
                    //all = число всех симптомов в строке, соответствующей поставленному диагнозу
                    int all = MassiveOfSymptoms(Strings[i]).Length;
                    double K = Confidence(max, all); //степень уверенности системы в результате
                    MasOfConfidence[p] = K;
                    scr();
                    p++; }}
            print("\n"); scr();

            string forecast=" - ", med_exam=" - ", treatm=" - ";

            if (p == 1){ //если найден только один подходящий диагноз
                //Diagnosis_Found=true;
                Supposed_Diagnosis = devide_diagnosis(Strings[maxIndex]).ToString();
                print("Система нашла наибольшее совпадение симптовов по диагнозу:\n" + 
                    Supposed_Diagnosis.ToUpper() + "\n");

                if (MasOfConfidence[0] <= 5) { print("  [коэффициент уверенности слишком низкий]\n"); scr(); }
                if (MasOfConfidence[0] >=6 && MasOfConfidence[0] <= 19) { print("  [коэффициент уверенности низкий]\n"); scr(); }
                if (MasOfConfidence[0] >= 70 && MasOfConfidence[0] <= 90) { print("  [коэффициент уверенности достаточно высокий]\n"); scr(); }
                if (MasOfConfidence[0] >= 91) { print("  [коэффициент уверенности высокий]\n"); scr(); }

                scr(); print("\n"); scr();
                //прогноз заболевания
                string[] Forecast = System.IO.File.ReadAllLines(@".\Knowledge_Base\forecast.txt", Encoding.Default);
                print("- ПРОГНОЗ:\n"); scr();
                print("   " + delete_diagnosis(Forecast[maxIndex]) + "\n"); scr();
                forecast = delete_diagnosis(Forecast[maxIndex]);
                //рекомендации по необходимым обследованиям
                string[] Med_examinations = System.IO.File.ReadAllLines(@".\Knowledge_Base\med_examinations.txt", 
                    Encoding.Default);
                print("\n- НЕОБХОДИМЫЕ ОБСЛЕДОВАНИЯ:\n"); scr();
                print("   "+delete_diagnosis(Med_examinations[maxIndex]) + "\n"); scr();
                med_exam = delete_diagnosis(Med_examinations[maxIndex]);
                //рекомендации по лечению
                string[] Treatment = System.IO.File.ReadAllLines(@".\Knowledge_Base\treatment.txt", Encoding.Default);
                print("\n- ЛЕЧЕНИЕ:\n"); scr();
                print("   " + delete_diagnosis(Treatment[maxIndex]) + "\n"); scr();
                treatm = delete_diagnosis(Treatment[maxIndex]); }
            scr();

            //если найдено несколько подходящих диагнозов:
            if (p > 1){
                print("Система нашла наибольшее количество совпадений по нескольким разным диагнозам.\n");
                scr(); print("\n"); scr();
                //поиск максимального коэффициента уверенности
                double maxK = MasOfConfidence[0]; int maxIndexK = 0;
                for (int m = 0; m < Strings.Length; m++){
                    if (MasOfConfidence[m] > maxK)
                    { //если найден больший коэфф-т уверенности
                        maxK = MasOfConfidence[m];
                        maxIndexK = m; }}
                //поиск повторов максимальных коэф., т.е. нескольких подходящих диагнозов
                bool Many = false; int X = 0;
                for (int m = 0; m < Strings.Length; m++) {
                    if (MasOfConfidence[m] == maxK){ //если найден такой же коэфф-т уверенности
                        X++;}}
                if (X > 1) Many = true; //диагнозов несколько по коэф.уверенности
                if (Many == false) { //диагноз только один по коэф.уверенности
                    //Diagnosis_Found=true; 
                    Supposed_Diagnosis = devide_diagnosis(Supposed_Diagnoses[maxIndexK]).ToString();
                    print("Наиболее подходящим из них кажется диагноз:\n" 
                        + devide_diagnosis(Supposed_Diagnoses[maxIndexK]).ToString().ToUpper() + "\n");
                    //print(maxIndexK.ToString());

                    if (maxK <= 5) { print("  [коэффициент уверенности слишком низкий]\n"); scr(); }
                    if (maxK >= 6 && MasOfConfidence[0] <= 19) { print("  [коэффициент уверенности низкий]\n"); scr(); }
                    if (maxK >= 70 && MasOfConfidence[0] <= 90) { print("  [коэффициент уверенности достаточно высокий]\n"); scr(); }
                    if (maxK >= 91) { print("  [коэффициент уверенности высокий]\n"); scr(); }

                    print("\n"); scr();
                    //массив всех диагнозов
                    string[] All_Diagnoses = System.IO.File.ReadAllLines(@".\Knowledge_Base\all_diagnoses.txt", 
                        Encoding.Default);
                    int Number=0;//индекс найденного диагноза для общего массива диагнозов
                    for (int d=0; d<All_Diagnoses.Length; d++){ 
                        if (Supposed_Diagnosis == All_Diagnoses[d]) Number = d; }
                    //прогноз заболевания
                    string[] Forecast = System.IO.File.ReadAllLines(@".\Knowledge_Base\forecast.txt", Encoding.Default);
                    print("- ПРОГНОЗ:\n"); scr();
                    print("   " + delete_diagnosis(Forecast[Number]) + "\n"); scr();
                    forecast = delete_diagnosis(Forecast[Number]);
                    //рекомендации по необходимым обследованиям
                    string[] Med_examinations = System.IO.File.ReadAllLines(@".\Knowledge_Base\med_examinations.txt", 
                        Encoding.Default);
                    print("\n- НЕОБХОДИМЫЕ ОБСЛЕДОВАНИЯ:\n"); scr();
                    print("   " + delete_diagnosis(Med_examinations[Number]) + "\n"); scr();
                    med_exam = delete_diagnosis(Med_examinations[Number]);
                    //рекомендации по лечению
                    string[] Treatment = System.IO.File.ReadAllLines(@".\Knowledge_Base\treatment.txt", Encoding.Default);
                    print("\n- ЛЕЧЕНИЕ:\n"); scr();
                    print("   " + delete_diagnosis(Treatment[Number]) + "\n"); scr();
                    treatm = delete_diagnosis(Treatment[Number]);}
                else { print("Требуется мнение медицинского специалиста.\n"); scr(); } }

            //СОХРАНЕНИЕ ДАННЫХ В БД
            //***************************************
            if (checkBox2.Checked == true){//если сохранение включено
                string name = "";
                if (NameTextBox.Text == "") name = "Anonymous"; else name = NameTextBox.Text;
                int age = (int)numericUpDown1.Value;
                string gender = "Не указан";
                if (radioButton1.Checked == true) gender = "М";
                else if (radioButton2.Checked == true) gender = "Ж";
                string date = DateTime.Now.ToString("g");
                string symptoms = "";
                foreach (string str in M1) symptoms = symptoms + str + ", ";
                symptoms = symptoms.Remove(symptoms.Length - 2);
                string diagnos = Supposed_Diagnosis;
                string med_examination = med_exam;
                string treatment = treatm;
                Save_in_DataBase(name, age, gender, date, symptoms, diagnos, med_examination, treatment);
                print("\n"); scr();
                print("[Данные занесены в БД]\n"); }
            //***************************************

            //тест на время выполнения функции (окончание)
            //stopWatch.Stop();
            //print("\n-----\nВремя выполнения процесса диагностики: " + 
            //    ((double)stopWatch.ElapsedMilliseconds / 1000).ToString() + " сек"); scr();
        }
        //************************************************************

        //Ф-Я РАССЧЕТА КОЭФФИЦИЕНТА УВЕРЕННОСТИ
        public double Confidence(int k, int K) {
            //косинусоидальная ф-я расчета коэффициента
            double Coefficient = (double)((double)-1 / 2 * Math.Cos(Math.PI / K * k) + 0.5) * 100;
            if (Coefficient < 1) Coefficient = Math.Round(Coefficient, 1);
            else Coefficient = Math.Round(Coefficient, 0);
            print("  [коэффициент уверенности = -1/2*(Cos(pi/" + K.ToString() + "*" + k.ToString() 
                + ")+0.5)*100% =\n  = " + Coefficient.ToString() + "%]\n");
            scr();
            return Coefficient; }

        //Ф-Я ИМИТАЦИЯ ПЕЧАТИ
        public void print(string phrase){
            if (Fast == false){ //если быстрый вывод выключен, то запускается имитация печати
                SoundPlayer klav = new SoundPlayer(@".\sound\klav.wav");
                string[] M = new string[phrase.Length];
                klav.Play();
                for (int i = 0; i < phrase.Length; i++) {
                    M[i] = phrase.Substring(i, 1);
                    Thread.Sleep(15); //длина задержки между выводом каждого символа
                    OutputWindow.AppendText(M[i]);
                    OutputWindow.Refresh();
                    if ((i % 60 == 0) && (i != 0)) scr();} //криво работает, подумать как изменить
                klav.Stop();}
            else { OutputWindow.AppendText(phrase); }} //иначе просто вывод текста

        //скроллинг к курсору
        public void scr() {
            OutputWindow.ScrollToCaret();}

        //Ф-Я ОТДЕЛЕНИЯ ДИАГНОЗА ОТ СТРОКИ
        public static string devide_diagnosis(string str) {
            int k = 0;
            string diagnosis = "";
            string simb1 = "";
            while (simb1 != "*") {//отделяем диагноз от всей строки 
                diagnosis = diagnosis + simb1;
                simb1 = str.Substring(k, 1);
                k = k + 1;}
            return diagnosis;}

        //Ф-Я ВОЗВРАЩАЮЩАЯ СТРОКУ БЕЗ ДИАГНОЗА (для рекомендаций по поводу диагноза)
        public static string delete_diagnosis(string str) {
            string diagnosis = devide_diagnosis(str);
            string text = str.Replace((diagnosis+"*"), "");
            return text; }

        //Ф-Я, ВОЗВРАЩАЮЩАЯ МАССИВ СИМПТОМОВ ИЗ СТРОКИ
        public static string[] MassiveOfSymptoms(string str) {
            string razd = "*";
            int indexOfRazd = str.IndexOf(razd);
            int AmountOfSimb = str.Length;
            int razd1 = 0;
            //подсчет количества вариантов ответа
            for (int i = 0; i < AmountOfSimb; i++) {
                if (str.Substring(i, 1) == "/") razd1 = razd1 + 1;} //сперва считаем кол-во разделителей
            int AmoutOfSymptoms;
            AmoutOfSymptoms = razd1 + 1; //кол-во симптомов в строке (на 1 больше чем разделителей)
            string[] MasOfSymptoms = new string[AmoutOfSymptoms]; //создадим массив симптомов строки
            int k = indexOfRazd + 1; string simb = "";
            for (int i = 0; i < AmoutOfSymptoms; i++) {
                while ((simb != "/") && (k < str.Length)){ //отделяем диагноз от всей строки
                    MasOfSymptoms[i] = MasOfSymptoms[i] + simb;
                    simb = str.Substring(k, 1);
                    k = k + 1;}
                if ((i + 1) == (AmoutOfSymptoms)) MasOfSymptoms[i] = MasOfSymptoms[i] + simb; //прибавление послед.символа если это посл.слово во фразе
                simb = ""; }
            return MasOfSymptoms;}

        //РИСОВАНИЕ РАМКИ ВОКРУГ ФОРМЫ
        private void Form1_Paint(object sender, PaintEventArgs e) {
            Graphics gr = e.Graphics;
            Pen p = new Pen(Color.Silver, 4);// цвет линии и ширина
            e.Graphics.DrawRectangle(p, e.ClipRectangle);//рисуем прямоугольник
            gr.Dispose(); }// освобождаем все ресурсы, связанные с отрисовкой

        //ВЫХОД ИЗ ПРОГРАММЫ
        private void button2_Click(object sender, EventArgs e){          
            Application.Exit();}           

        //Ф-Я СОХРАНЕНИЯ ИНФОРМАЦИИ В БД
        public void Save_in_DataBase(string name, int age, string gender, 
            string date, string symptoms, string diagnosis, string med_examination, string treatment){
            using (var Connectin = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=.\Database\database.mdb")){
                Connectin.Open();
                using (OleDbCommand command = Connectin.CreateCommand()){
                    command.CommandText = @"INSERT INTO [Anamnesis] ([ФИО],[Возраст],[Пол],[Дата обращения],[Жалобы],
                    [Предположительный диагноз],[Необходимые обследования],[Рекомендуемое лечение]) 
                    values ('" + name + "', '" + age + "', '" + gender + "', '" + date + "', '" 
                    + symptoms + "', '" + diagnosis + "', '" + med_examination + "', '" + treatment + "')";
                    command.ExecuteNonQuery();}
                Connectin.Close();}}

        //СБРОС ВЫБРАННЫХ СИМПТОМОВ
        private void button3_Click(object sender, EventArgs e){

            //вывод сообщения для защиты от непредвиденной потери данных о результатах диагностики
            if (OutputWindow.Text.Contains("ПОИСК СООТВЕТСТВИЯ") == true){
                MessageAmount++;
                DialogResult result = MessageBox.Show(MessageText(MessageAmount),
                    "Предупреждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (MessageAmount == 21) Application.Exit();
                if (result == DialogResult.No) { this.TopMost = true; return; }
                this.TopMost = true; }
         
            MessageOn = true;
            for (int Element = 0; Element < SymptomList.Items.Count; Element++)
                SymptomList.SetItemChecked(Element, false);
            SymptomList.SetSelected(0, false);
            NameTextBox.Text = ""; SearchTextBox.Text = "";
            OutputWindow.Text = "Окно вывода результатов...";
            MessageOn=false; 
            MessageAmount = 0; }     

        //Ф-Я БЫСТРОГО ПОИСКА СИМПТОМА СРЕДИ СПИСКА ПРИ ПЕЧАТАНИИ
        private void textBox2_KeyUp(object sender, KeyEventArgs e){ 
            foreach (object Item in SymptomList.Items){
                if (Item.ToString().Contains(SearchTextBox.Text) == true && SymptomList.FindString(SearchTextBox.Text, 0)!=-1)
                { SymptomList.SetSelected(SymptomList.FindString(SearchTextBox.Text, 0), true); break;} }
            if (SearchTextBox.Text == "") SymptomList.SetSelected(0, false);
            //при нажатии на Enter для установления/снятия галочки
            if (e.KeyCode == Keys.Enter && SymptomList.SelectedItem != null && 
                SymptomList.GetItemChecked(SymptomList.SelectedIndex) == false) 
                SymptomList.SetItemChecked(SymptomList.SelectedIndex, true);
            else if (e.KeyCode == Keys.Enter && SymptomList.SelectedItem != null && 
                SymptomList.GetItemChecked(SymptomList.SelectedIndex)==true)
                SymptomList.SetItemChecked(SymptomList.SelectedIndex, false); }

        //событие: Ф-Я ОТОБРАЖЕНИЯ ВЫБРАННЫХ СИМПТОМОВ В ТЕКСТОВОМ ОКНЕ
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e){
            //вывод сообщения для защиты от непредвиденной потери данных о результатах диагностики
            if (MessageOn==false)
            if (OutputWindow.Text.Contains("ПОИСК СООТВЕТСТВИЯ") == true){
                MessageAmount++;
                DialogResult result = MessageBox.Show(MessageText(MessageAmount), 
                    "Предупреждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (MessageAmount == 21) Application.Exit();
                if (result == DialogResult.No) { this.TopMost = true; return; }
                this.TopMost = true; }

                ProtectionOfButton = false; //отключение защиты кнопки, чтобы можно было проводить диагностику снова

                OutputWindow.Clear();
                OutputWindow.Text = OutputWindow.Text.Replace("Окно вывода результатов...", "");
                OutputWindow.Text = "Выбранные симптомы: ";

                //переопределенная функция обработчика события, чтобы оно срабатывало не после обновления, а во время него
                List<string> checkedItems = new List<string>();
                foreach (var item in SymptomList.CheckedItems) checkedItems.Add(item.ToString());
                if (e.NewValue == CheckState.Checked) checkedItems.Add(SymptomList.Items[e.Index].ToString());
                else checkedItems.Remove(SymptomList.Items[e.Index].ToString());
                SoundPlayer sound = new SoundPlayer(@".\sound\sound2.wav");
                sound.Play();    //sound.Stop();       
                foreach (string item in checkedItems)
                    OutputWindow.AppendText(item + "; ");
                if (OutputWindow.Text != "Выбранные симптомы: ")
                    OutputWindow.Text = OutputWindow.Text.Remove(OutputWindow.Text.Length - 2);//обрезать последнюю " ;"
                OutputWindow.Refresh();
                MessageAmount = 0; }        
    
        //Ф-ИИ ЗАДАНИЯ НУЖНОЙ ЦВЕТОВОЙ СХЕМЫ ПРИ ОПРЕДЕЛЕННЫХ ТЕМАХ
        private void MatrixHack_Theme() {
            this.BackColor = Color.Black; this.BackgroundImage = null;
            label1.ForeColor = Color.Lime; label1.BackColor = Color.Black;
            label2.ForeColor = Color.Lime; label2.BackColor = Color.Black;
            label4.ForeColor = Color.Lime; label4.BackColor = Color.Black;
            label5.ForeColor = Color.Lime; label5.BackColor = Color.Black;
            label3.ForeColor = Color.FromArgb(224, 224, 224); label3.BackColor = Color.Black;
            NameTextBox.ForeColor = Color.Lime; NameTextBox.BackColor = Color.Black;
            SearchTextBox.ForeColor = Color.Lime; SearchTextBox.BackColor = Color.Black;
            DiagnosticsButton.ForeColor = Color.Lime; DiagnosticsButton.BackColor = Color.Black;
            ExitButton.ForeColor = Color.FromArgb(224, 224, 224); ExitButton.BackColor = Color.Black;
            ClearButton.ForeColor = Color.FromArgb(224, 224, 224); ClearButton.BackColor = Color.Black;
            checkBox1.ForeColor = Color.Lime; checkBox1.BackColor = Color.Black;
            checkBox2.ForeColor = Color.Lime; checkBox2.BackColor = Color.Black;
            OutputWindow.ForeColor = Color.Lime; OutputWindow.BackColor = Color.Black;
            radioButton1.ForeColor = Color.FromArgb(224, 224, 224); radioButton1.BackColor = Color.Black;
            radioButton2.ForeColor = Color.FromArgb(224, 224, 224); radioButton2.BackColor = Color.Black;
            numericUpDown1.ForeColor = Color.Lime; numericUpDown1.BackColor = Color.Black;
            SymptomList.ForeColor = Color.FromArgb(224, 224, 224); SymptomList.BackColor = Color.Black;
            comboBox1.ForeColor = Color.FromArgb(224, 224, 224); comboBox1.BackColor = Color.Black;
            comboBox1.FlatStyle = FlatStyle.Flat; }

        private void LightClassic_Theme(){
            this.BackColor = Color.WhiteSmoke; this.BackgroundImage = null;
            label1.ForeColor = Color.Black; label1.BackColor = Color.WhiteSmoke;
            label2.ForeColor = Color.Black; label2.BackColor = Color.WhiteSmoke;
            label4.ForeColor = Color.Black; label4.BackColor = Color.WhiteSmoke;
            label5.ForeColor = Color.Black; label5.BackColor = Color.WhiteSmoke;
            label3.ForeColor = Color.Black; label3.BackColor = Color.WhiteSmoke;
            NameTextBox.ForeColor = Color.Black; NameTextBox.BackColor = Color.White;
            SearchTextBox.ForeColor = Color.Black; SearchTextBox.BackColor = Color.White;
            DiagnosticsButton.ForeColor = Color.Navy; DiagnosticsButton.BackColor = Color.White;
            ExitButton.ForeColor = Color.Black; ExitButton.BackColor = Color.WhiteSmoke;
            ClearButton.ForeColor = Color.Black; ClearButton.BackColor = Color.White;
            checkBox1.ForeColor = Color.Black; checkBox1.BackColor = Color.WhiteSmoke;
            checkBox2.ForeColor = Color.Black; checkBox2.BackColor = Color.WhiteSmoke;
            OutputWindow.ForeColor = Color.Black; OutputWindow.BackColor = Color.White;
            radioButton1.ForeColor = Color.Navy; radioButton1.BackColor = Color.WhiteSmoke;
            radioButton2.ForeColor = Color.Navy; radioButton2.BackColor = Color.WhiteSmoke;
            numericUpDown1.ForeColor = Color.Black; numericUpDown1.BackColor = Color.White;
            SymptomList.ForeColor = Color.Navy; SymptomList.BackColor = Color.White;
            comboBox1.ForeColor = Color.Black; comboBox1.BackColor = Color.White;
            comboBox1.FlatStyle = FlatStyle.Standard;}

        private void DarkOld_Theme(){
            this.BackColor = Color.Black; this.BackgroundImage = null;
            label1.ForeColor = Color.Goldenrod; label1.BackColor = Color.Black;
            label2.ForeColor = Color.Goldenrod; label2.BackColor = Color.Black;
            label4.ForeColor = Color.Goldenrod; label4.BackColor = Color.Black;
            label5.ForeColor = Color.Goldenrod; label5.BackColor = Color.Black;
            label3.ForeColor = Color.FromArgb(224, 224, 224); label3.BackColor = Color.Black;
            NameTextBox.ForeColor = Color.FromArgb(224, 224, 224); NameTextBox.BackColor = Color.FromArgb(12, 12, 12);
            SearchTextBox.ForeColor = Color.FromArgb(224, 224, 224); SearchTextBox.BackColor = Color.FromArgb(12, 12, 12);
            DiagnosticsButton.ForeColor = Color.Goldenrod; DiagnosticsButton.BackColor = Color.FromArgb(12, 12, 12);
            ExitButton.ForeColor = Color.FromArgb(224, 224, 224); ExitButton.BackColor = Color.FromArgb(12, 12, 12);
            ClearButton.ForeColor = Color.FromArgb(224, 224, 224); ClearButton.BackColor = Color.FromArgb(12, 12, 12);
            checkBox1.ForeColor = Color.Goldenrod; checkBox1.BackColor = Color.Black;
            checkBox2.ForeColor = Color.Goldenrod; checkBox2.BackColor = Color.Black;
            OutputWindow.ForeColor = Color.Goldenrod; OutputWindow.BackColor = Color.FromArgb(12, 12, 12);
            radioButton1.ForeColor = Color.FromArgb(224, 224, 224); radioButton1.BackColor = Color.Black;
            radioButton2.ForeColor = Color.FromArgb(224, 224, 224); radioButton2.BackColor = Color.Black;
            numericUpDown1.ForeColor = Color.FromArgb(224, 224, 224); numericUpDown1.BackColor = Color.FromArgb(12, 12, 12);
            SymptomList.ForeColor = Color.FromArgb(224, 224, 224); SymptomList.BackColor = Color.FromArgb(12, 12, 12);
            comboBox1.ForeColor = Color.FromArgb(224, 224, 224); comboBox1.BackColor = Color.FromArgb(12, 12, 12);
            comboBox1.FlatStyle = FlatStyle.Flat; }

        private void KawaiiPink_Theme() {
            this.BackColor = Color.MistyRose;
            label1.ForeColor = Color.Black; label1.BackColor = Color.Transparent; 
            label2.ForeColor = Color.Black; label2.BackColor = Color.Transparent; 
            label4.ForeColor = Color.Black; label4.BackColor = Color.Transparent; 
            label5.ForeColor = Color.Black; label5.BackColor = Color.Transparent;
            label3.ForeColor = Color.FromArgb(0, 80, 0); label3.BackColor = Color.MistyRose; 
            NameTextBox.ForeColor = Color.Black; NameTextBox.BackColor = Color.White;
            SearchTextBox.ForeColor = Color.Black; SearchTextBox.BackColor = Color.White;
            DiagnosticsButton.ForeColor = Color.FromArgb(0, 80, 0); DiagnosticsButton.BackColor = Color.White;
            ExitButton.ForeColor = Color.Black; ExitButton.BackColor = Color.WhiteSmoke;
            ClearButton.ForeColor = Color.Black; ClearButton.BackColor = Color.White;
            checkBox1.ForeColor = Color.Black; checkBox1.BackColor = Color.Transparent;
            checkBox2.ForeColor = Color.Black; checkBox2.BackColor = Color.Transparent; 
            OutputWindow.ForeColor = Color.Black; OutputWindow.BackColor = Color.White;
            radioButton1.ForeColor = Color.FromArgb(0, 80, 0); radioButton1.BackColor = Color.Transparent;
            radioButton2.ForeColor = Color.FromArgb(0, 80, 0); radioButton2.BackColor = Color.Transparent;
            numericUpDown1.ForeColor = Color.Black; numericUpDown1.BackColor = Color.White;
            SymptomList.ForeColor = Color.FromArgb(0, 80, 0); SymptomList.BackColor = Color.White;
            comboBox1.ForeColor = Color.Black; comboBox1.BackColor = Color.White;
            comboBox1.FlatStyle = FlatStyle.Standard;
            this.BackgroundImage = Image.FromFile(@".\pictures\kitty.png"); }

        private void GrayMouse_Theme(){
            this.BackColor = Color.FromArgb(64, 64, 64); this.BackgroundImage = null;
            label1.ForeColor = Color.FromArgb(224, 224, 224); label1.BackColor = Color.FromArgb(64, 64, 64);
            label2.ForeColor = Color.FromArgb(224, 224, 224); label2.BackColor = Color.FromArgb(64, 64, 64);
            label4.ForeColor = Color.FromArgb(224, 224, 224); label4.BackColor = Color.FromArgb(64, 64, 64);
            label5.ForeColor = Color.FromArgb(224, 224, 224); label5.BackColor = Color.FromArgb(64, 64, 64);
            label3.ForeColor = Color.FromArgb(224, 224, 224); label3.BackColor = Color.FromArgb(64, 64, 64);
            NameTextBox.ForeColor = Color.White; NameTextBox.BackColor = Color.Gray;
            SearchTextBox.ForeColor = Color.White; SearchTextBox.BackColor = Color.Gray;
            DiagnosticsButton.ForeColor = Color.DarkBlue; DiagnosticsButton.BackColor = Color.White;
            ExitButton.ForeColor = Color.Black; ExitButton.BackColor = Color.White;
            ClearButton.ForeColor = Color.Black; ClearButton.BackColor = Color.White;
            checkBox1.ForeColor = Color.FromArgb(224, 224, 224); checkBox1.BackColor = Color.FromArgb(64, 64, 64);
            checkBox2.ForeColor = Color.FromArgb(224, 224, 224); checkBox2.BackColor = Color.FromArgb(64, 64, 64);
            OutputWindow.ForeColor = Color.White; OutputWindow.BackColor = Color.Gray;
            radioButton1.ForeColor = Color.FromArgb(224, 224, 224); radioButton1.BackColor = Color.FromArgb(64, 64, 64);
            radioButton2.ForeColor = Color.FromArgb(224, 224, 224); radioButton2.BackColor = Color.FromArgb(64, 64, 64);
            numericUpDown1.ForeColor = Color.White; numericUpDown1.BackColor = Color.Gray;
            SymptomList.ForeColor = Color.DarkBlue; SymptomList.BackColor = Color.Gray;
            comboBox1.ForeColor = Color.White; comboBox1.BackColor = Color.Gray;
            comboBox1.FlatStyle = FlatStyle.Flat; }

        //событие: смена темы при переключении темы и сохранение номера выбранной темы в файл
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e){
            int Theme = comboBox1.SelectedIndex; //считываем номер темы          
            StreamWriter FileSW = new StreamWriter("theme.txt");
            FileSW.WriteLine(Theme.ToString()); //записываем номер в файл для сохранения
            FileSW.Close();
            SelectTheme(Theme); } //устанавливаем тему

        //Ф-Я установки нужной темы по передаваему номеру темы (выбранной или сохраненной)
        private void SelectTheme(int ThemeNumber) {
            if (ThemeNumber == 0) MatrixHack_Theme();
            if (ThemeNumber == 1) LightClassic_Theme();
            if (ThemeNumber == 2) DarkOld_Theme();
            if (ThemeNumber == 3) KawaiiPink_Theme();
            if (ThemeNumber == 4) GrayMouse_Theme();}

        //Ф-Я УСТАНОВЛЕНИЯ/СНЯТИЯ ФЛАЖКА У ВЫБРАННОГО ЭЛЕМЕНТА ПРИ НАЖАТИИ НА ENTER
        private void checkedListBox1_KeyUp(object sender, KeyEventArgs e){
            //программный вызов события при нажатии на Enter
            if (e.KeyCode == Keys.Enter && SymptomList.SelectedItem != null 
                && SymptomList.GetItemChecked(SymptomList.SelectedIndex) == false) 
                SymptomList.SetItemChecked(SymptomList.SelectedIndex, true);
            else if (e.KeyCode == Keys.Enter && SymptomList.SelectedItem != null 
                && SymptomList.GetItemChecked(SymptomList.SelectedIndex)==true)
                SymptomList.SetItemChecked(SymptomList.SelectedIndex, false);}

        //смена и сохранение режима быстрого вывода
        private void checkBox1_CheckedChanged(object sender, EventArgs e){
            //быстрый или медленный вывод результатов
            if (checkBox1.Checked == true) Fast = true; else Fast = false;
            StreamWriter FileSW1 = new StreamWriter("fast.txt");
            FileSW1.WriteLine(Fast.ToString()); //записываем режим в файл для сохранения
            FileSW1.Close(); }

        //проверка ввода символов в поле ФИО, чтобы не ввели спецсимволы и т.п.
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e){
            //разрешаем бэкспейс, дефис, точку и пробел для инициалов
            if (e.KeyChar.Equals('\b')) return;
            if (e.KeyChar.Equals('-')) return;
            if (e.KeyChar.Equals('.')) return;
            if (e.KeyChar.Equals(' ')) return;
            //разрешаем только ввод букв
            e.Handled = !char.IsLetter(e.KeyChar);}

        //смена и сохранение режима записи данных в БД
        private void checkBox2_CheckedChanged(object sender, EventArgs e){
            if (checkBox2.Checked == true) SaveToDB = true; else SaveToDB = false;
            StreamWriter FileSW2 = new StreamWriter("save_to_DB.txt");
            FileSW2.WriteLine(SaveToDB.ToString()); //записываем режим в файл для сохранения
            FileSW2.Close();}

        //проверка ввода символов в поле быстрого поиска
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e){
            //резрешаем бэкспейс и пробел
            if (e.KeyChar.Equals('\b')) return;
            if (e.KeyChar.Equals(' ')) return;
            //разрешаем только ввод букв
            e.Handled = !char.IsLetter(e.KeyChar);}

        //текст предупредительного сообщения
        private static string MessageText(int number){
            string msg = "Текущий результат будет потерян. Продолжить?";
            if (number == 1) msg = "Текущий результат будет потерян. Продолжить?";
            if (number == 2) msg = "Текущий результат правда будет потерян. Продолжить?";
            if (number == 3) msg = "Еще раз объясняю, что текущий результат будет потерян. Продолжить?";
            if (number == 4) msg = "На случай, если непонятно, что текущий результат будет потерян, еще раз спрашиваю: продолжить?";
            if (number == 5) msg = "Тебе точно так хочется потерять текущий результат?";
            if (number == 6) msg = "Похоже, что желание потерять текущий результат смахивает на упрямство.";
            if (number == 7) msg = "Иными словами, текущий результат не будет потерян, пока ты нажимаешь нет.";
            if (number == 8) msg = "На нет и суда нет, следовательно ты хочешь сохранить текущий результат.";
            if (number == 9) msg = "Я тебя не понимаю. Ты хочешь продолжить или нет?";
            if (number == 10) msg = "Тебе надо разобраться в себе.";
            if (number == 11) msg = "Ты колеблешься?";
            if (number == 12) msg = "Не стоит колебаться. Продолжаем?";
            if (number == 13) msg = "Неужели ты настолько неуверенный человек?";
            if (number == 14) msg = "Скоро ли настанет тот момент, когда ты решишься потерять текущий результат?";
            if (number == 15) msg = "Текущий результат так или иначе когда-то будет потерян. Нужно идти дальше.";
            if (number == 16) msg = "Мы стоим на месте.";
            if (number == 17) msg = "Кажется, это незакрытый Гештальт.";
            if (number == 18) msg = "Пожалуй, мир превратится в пустыню быстрее, чем ты решишься расстаться с текущим результатом.";
            if (number == 19) msg = "Тебе действительно так сильно нужно сохранить текущий результат?";
            if (number == 20) msg = "Что-то я устал. А ты?";
            if (number > 20) msg = "//искусственный интеллект выпал в осадок...";
            return msg;}  
    }}
