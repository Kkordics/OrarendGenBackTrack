using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace OrarendFinal2._0
{

    public static class Help
    {

        public static int Summ(this List<byte> input)
        {
            int handler = 0;

            for (int i = 0; i < input.Count; i++)
            {
                handler += input[i];
            }
            return handler;
        }

        public static byte ToByte(this int i)
        {

            if (i < 0)
            {
                throw new Exception(i.ToString());
            }

            return Convert.ToByte(i);
        }
        public static List<string> GetNames(this List<TableElements.Tanar> tanarok)
        {
            List<string> handler = new List<string>();

            for (int i = 0; i < tanarok.Count; i++)
            {
                handler.Add(tanarok[i].Nev);
            }
            return handler;
        }
        /// <summary>
        /// Vissza adja azokat a tanárokat akik egy adott napon egy órában a megadott osztályhoz képest balra tanítottak
        /// </summary>
        /// <param name="osztaly"></param>
        /// <param name="dayIndex"></param>
        /// <param name="acsoport"></param>
        /// <param name="oraIndex"></param>
        /// <returns></returns>
        public static List<string> GetTeacherNames(this List<TableElements.Osztaly> osztaly, byte dayIndex, byte oraIndex)
        {
            List<string> handler = new List<string>();

            for (int i = 0; i < osztaly.Count; i++)
            {

                handler.Add(osztaly[i].OraTipusok[osztaly[i].Napok[dayIndex].Orak[oraIndex].OraIndex].Tanar.Nev);

            }
            return handler;
        }
    }

    public static class Gen
    {


        public class Coord
        {
            public byte osztalyIndex;
            public byte DayIndex;
            public byte OraIndex;

            public string ToString()
            {
                return string.Format("Class: {0} | Day: {1}| Ora: {2}", osztalyIndex, DayIndex, OraIndex);

            }

        }
        /// <summary>
        /// Vissza adja a kövi tanárt a listába ha van
        /// </summary>
        /// <param name="tanar"></param>
        /// <param name="tanarok"></param>
        /// <returns></returns>
        private static TableElements.Tanar GetNextTeacher(TableElements.Tanar tanar, List<TableElements.Tanar> tanarok)
        {
            TableElements.Tanar next = new TableElements.Tanar();

            for (int i = 0; i < tanarok.Count; i++)
            {
                if (tanar.Nev == tanarok[i].Nev)
                {
                    if (i + 1 <= tanarok.Count - 1 && i != tanarok.Count - 1)
                    {
                        next = tanarok[i + 1];
                    }
                    else
                    {
                        break;
                    }

                }
            }

            return next;

        }

        public static List<string> GetTeachersNames(List<TableElements.Tanar> tanarok, Coord coord)
        {


            List<string> handler = new List<string>();

            for (int i = 0; i < coord.osztalyIndex; i++)
            {
                if (tanarok[i].OraTypeIndexes[coord.osztalyIndex].Count != 0)
                {
                    handler.Add(tanarok[i].Nev);
                }

            }
            return handler;
        }

        /// <summary>
        /// Vissza adja azokat a tanárokat a listából akik még eddig nem tanítottak
        /// </summary>
        /// <returns></returns>
        public static List<TableElements.Tanar> GetTeachers(List<TableElements.Tanar> tanarok, List<TableElements.Osztaly> osztalyok, Coord coord)
        {


            List<TableElements.Tanar> handler = new List<TableElements.Tanar>();


            //Kigyűjti hogy kik tanítottak eddig ha tanítottak
            if (coord.osztalyIndex != 0)
            {
                List<string> h2 = new List<string>();

                for (int i = 0; i < coord.osztalyIndex; i++)
                {


                    if (h2.Contains(osztalyok[i].OraTipusok[osztalyok[i].Napok[coord.DayIndex].Orak[coord.OraIndex].OraIndex].Tanar.Nev) == false)
                    {
                        h2.Add(osztalyok[i].OraTipusok[osztalyok[i].Napok[coord.DayIndex].Orak[coord.OraIndex].OraIndex].Tanar.Nev);


                        //Console.Write("{0}|", osztalyok[i].OraTipusok[osztalyok[i].Napok[coord.DayIndex].AOrak[coord.OraIndex].OraIndex].Tanar.Nev);
                    }




                }
                //Console.WriteLine();

                for (int i = 0; i < tanarok.Count; i++)
                {
                    if (h2.Contains(tanarok[i].Nev) == false)
                    {
                        handler.Add(tanarok[i]);
                    }
                }

            }
            else
            {
                handler = tanarok;
            }

            return handler;
        }


        private static List<TableElements.Tanar> SortBy(this List<TableElements.Tanar> tanarok, List<List<byte>> oragyak)
        {

            //Szinkrunban van a tanárokkal
            List<int> sums = new List<int>();
            for (int i = 0; i < oragyak.Count; i++)
            {
                sums.Add(oragyak[i].Summ());
            }


            List<TableElements.Tanar> handler = new List<TableElements.Tanar>();
            int index = 0;
            for (int i = 0; i < sums.Count; i++)
            {
                index = sums.IndexOf(sums.Min());

                handler.Add(tanarok[index]);
                sums[index] = sums.Max() + 1;

            }



            return handler;
        }



        /// <summary>
        /// Fontossági sorrendbe rakja a tanárok óráit
        /// És kiszedi azokat az órákat amiből elég van
        /// </summary>
        /// <param name="osztaly"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        private static List<TableElements.Tanar> LessonPrioriti(TableElements.Osztaly osztaly, List<TableElements.Tanar> tanarok, Coord coord)
        {

            List<TableElements.Tanar> handler = new List<TableElements.Tanar>();

            //szinkronba van a tanárokkal
            byte[] tanitasok = new byte[tanarok.Count];

            List<byte> handler2 = new List<byte>();
            List<byte> handler3 = new List<byte>();

            TableElements.Tanar tanar = new TableElements.Tanar();


            //1. dimenzió a tanárokkal van szinkronba
            //2. dimenzió az adott tanár óráival van szinkronba és azt tárolja hányszor volt tartva az óra

            List<List<byte>> orakgyakorisaga = new List<List<byte>>();

            
            #region 2d lsita alkotás

            List<byte> handler2d = new List<byte>();


            for (int i = 0; i < tanarok.Count; i++)
            {
                handler2d = new List<byte>();
                for (int a = 0; a < tanarok[i].OraTypeIndexes[coord.osztalyIndex].Count; a++)
                {
                    handler2d.Add(0);
                }


                orakgyakorisaga.Add(handler2d);

            }

            #endregion


            string namehandler = string.Empty;
            int indexopt = 0;

            //Vagyis nem Hétfő van
            if (coord.DayIndex != 0)
            {
                //Console.WriteLine("itt");
                #region Tanítás kigyűjtése


                //Kigyűjti hogy ki mennyiszer tanított és hogy az óráit hányszor tanította
                for (int n = 0; n <= coord.DayIndex; n++)
                {
                    for (int o = 0; o < coord.OraIndex; o++)
                    {
                        namehandler = osztaly.OraTipusok[osztaly.Napok[n].Orak[o].OraIndex].Tanar.Nev;

                        //Ha az aktuális helyen lévő tanár benne van a listába akkor
                        if (tanarok.GetNames().Contains(namehandler))
                        {
                            indexopt = tanarok.GetNames().IndexOf(namehandler);

                            tanitasok[indexopt]++;
                            //Hozzáadunk egyet a óra gyakorisághoz mivel ez a z óra volt tartva 
                            orakgyakorisaga[indexopt][tanarok[indexopt].OraTypeIndexes[coord.osztalyIndex].IndexOf(osztaly.Napok[n].Orak[o].OraIndex)]++;
                        }
                    }
                }



                #endregion



                //Növekvő sorrendbe rakjuk a tanárokat és közbe elkészítjük a tanár óráit
                tanarok.SortBy(orakgyakorisaga);

                for (int i = 0; i < tanarok.Count; i++)
                {

                    handler2 = new List<byte>();
                    handler3 = new List<byte>();

                    for (int a = 0; a < tanarok[i].OraTypeIndexes[coord.osztalyIndex].Count && orakgyakorisaga[i].Count != 0; a++)
                    {
                        //Itt azt vizsgálja hogy az óra nem egyenlő a hét max előfordukásával
                        if (orakgyakorisaga[i][a] != osztaly.OraTipusok[tanarok[i].OraTypeIndexes[coord.osztalyIndex][a]].HetiElfordulas)
                        {
                            handler2.Add(tanarok[i].OraTypeIndexes[coord.osztalyIndex][a]);
                        }
                        else
                        {
                            orakgyakorisaga[i].RemoveAt(a);

                            //Console.WriteLine("Elfoggytak az órák");
                            //Console.WriteLine("{0}", osztaly.OraTipusok[tanarok[i].OraTypeIndexes[coord.osztalyIndex][a]].Oraneve);
                            a--;
                            //Console.ReadLine();
                        }
                    }

                    

                    //növekvő sorrendbe kell rakni ezeket az órákat a handler3 -ba

                    for (int a = 0; a < handler2.Count; a++)
                    {
                        handler3.Add(handler2[orakgyakorisaga[i].IndexOf(orakgyakorisaga[i].Min())]);


                        //A minimumot nem törölhetjük szóval az egyenlő lesz a maxal+1-el mert úgyis a lista hosszaszor fut le
                        orakgyakorisaga[i][orakgyakorisaga[i].IndexOf(orakgyakorisaga[i].Min())] = (byte)(orakgyakorisaga[i].Max() + 1);
                    }



                    if (handler3.Count != 0)
                    {
                        tanar = tanarok[i];

                        tanar.OraTypeIndexes[coord.osztalyIndex] = handler3;

                        handler.Add(tanar);
                    }


                }


            }
            else
            {
                #region Tanítás gyakoriság kigyüjtés

                //for (int i = 0; i < tanarok.Count; i++)
                //{
                //    for (int o = 0; o < coord.OraIndex; o++)
                //    {
                //        if (tanarok[i].OraTypeIndexes[coord.osztalyIndex].Contains(osztaly.Napok[0].AOrak[o].OraIndex))
                //        {
                //            tanitasok[i]++;

                //            orakgyakorisaga[i][tanarok[i].OraTypeIndexes[coord.osztalyIndex].IndexOf(osztaly.Napok[0].AOrak[o].OraIndex)]++;
                //        }

                //    }
                //}

                for (int o = 0; o < coord.OraIndex; o++)
                {
                    namehandler = osztaly.OraTipusok[osztaly.Napok[0].Orak[o].OraIndex].Tanar.Nev;

                    //Ha az aktuális helyen lévő tanár benne van a listába akkor
                    if (tanarok.GetNames().Contains(namehandler))
                    {
                        indexopt = tanarok.GetNames().IndexOf(namehandler);

                        tanitasok[indexopt]++;
                        //Hozzáadunk egyet a óra gyakorisághoz mivel ez a z óra volt tartva 

                        orakgyakorisaga[indexopt][tanarok[indexopt].OraTypeIndexes[coord.osztalyIndex].IndexOf(osztaly.Napok[0].Orak[o].OraIndex)]++;
                    }
                }

                #endregion


                if (coord.OraIndex != 0)
                {


                    tanarok = tanarok.SortBy(orakgyakorisaga);



                    for (int i = 0; i < tanarok.Count; i++)
                    {


                        handler2 = new List<byte>();
                        handler3 = new List<byte>();
                        for (int a = 0; a < tanarok[i].OraTypeIndexes[coord.osztalyIndex].Count && orakgyakorisaga[i].Count != 0; a++)
                        {
                            //Itt azt vizsgálja hogy az óra nem egyenlő a hét max előfordukásával
                            if (orakgyakorisaga[i][a] != osztaly.OraTipusok[tanarok[i].OraTypeIndexes[coord.osztalyIndex][a]].HetiElfordulas)
                            {
                                handler2.Add(tanarok[i].OraTypeIndexes[coord.osztalyIndex][a]);
                            }
                            else
                            {
                                orakgyakorisaga[i].RemoveAt(a);

                                a--;

                            }
                        }


                        

                        //növekvő sorrendbe kell rakni ezeket az órákat a handler3 -ba



                        for (int a = 0; a < handler2.Count; a++)
                        {
                            handler3.Add(handler2[orakgyakorisaga[i].IndexOf(orakgyakorisaga[i].Min())]);

                            //A minimumot nem törölhetjük szóval az egyenlő lesz a maxal+1-el mert úgyis a lista hosszaszor fut le
                            orakgyakorisaga[i][orakgyakorisaga[i].IndexOf(orakgyakorisaga[i].Min())] = (byte)(orakgyakorisaga[i].Max() + 1);
                        }


                        if (handler3.Count != 0)
                        {
                            tanar = tanarok[i];



                            tanar.OraTypeIndexes[coord.osztalyIndex] = handler3;

                            handler.Add(tanar);
                        }


                    }




                }
                else
                {
                    if (tanarok.Count != 0)
                    {


                        //1. lehetőség
                        //Lehet hogy valami prioritás szám kéne ami azt mutatja hogy az óra mennyire nehéz és akkor ezt nem rakja be az első órába hétfőn

                        //Itt hétfő első óra van
                        //Random rnd = new Random();

                        //int index = rnd.Next(0, tanarok.Count);

                        //TableElements.Tanar ran = tanarok[index];

                        //ran.OraTypeIndexes[coord.osztalyIndex][0] = ran.OraTypeIndexes[coord.osztalyIndex][rnd.Next(ran.OraTypeIndexes[coord.osztalyIndex].Count)];

                        for (int i = 0; i < tanarok.Count; i++)
                        {
                            handler.Add(tanarok[i]);
                        }

                        //handler.Add(ran);


                        //2. Lehetőség
                        //handler.Add(tanarok[0]);

                    }

                }
            }


            //Ha a egy tanár úgy tér vissza hogy nincs egy órája se akkor azt bele se rakja




            return handler;
        }

        //Jól mükszik
        /// <summary>
        /// Vissza adja az osztályba tanító tanárokat, és azokat akik ráérnek az adott órában és napon 
        /// 
        /// </summary>
        /// <param name="tanarok"></param>
        /// <returns></returns>
        private static List<TableElements.Tanar> GetClasssTeachers(List<TableElements.Tanar> tanarok, Coord coord, TableElements.Osztaly osztaly)
        {



            List<TableElements.Tanar> output = new List<TableElements.Tanar>();

            for (int i = 0; i < tanarok.Count; i++)
            {
                //Tanít az adoot osztályban
                if (tanarok[i].OraTypeIndexes[coord.osztalyIndex].Count != 0)
                {

                    //Rá is ér az adott időpontban
                    if (tanarok[i].Reszvetel[coord.DayIndex].FullDay == true || tanarok[i].Reszvetel[coord.DayIndex].StarterOra <= coord.OraIndex || tanarok[i].Reszvetel[coord.DayIndex].Endora > coord.OraIndex)
                    {

                        output.Add(tanarok[i]);
                    }

                }

            }


            return output;
        }

        public static List<TableElements.Osztaly> GenOneWeek(this List<TableElements.Osztaly> osztalyok, List<TableElements.Tanar> tanarok)
        {
            int h2 = 0;
            bool repeate2 = false;

            List<TableElements.Tanar> handler2 = new List<TableElements.Tanar>();

            TableElements.Ora handler3 = new TableElements.Ora();

            TableElements.Tanar htanar = new TableElements.Tanar();


            
            long bc = 0;
            //Ez mutatja hogy mejik osztálynak volt változás
            int bcindex = 0;
            bool fbc = false;
            bool change = false;
            int cha = 0;

            bool needChange = false;
            //Hiba lehetőség
            //Csak annyi órát generálunk amennyi minimum lehet
            #region Get smallest max lesson

            List<byte> handler = new List<byte>();
            for (int i = 0; i < osztalyok.Count; i++)
            {
                handler.Add(osztalyok[i].NapiMaxOrak);
            }

            #endregion

            for (int dayIndex = 0; dayIndex < 5; dayIndex++)//Napok
            {
                for (int i = 0; i < handler.Min(); i++)//Ciklus: 1
                {

                    for (int o = 0; o < osztalyok.Count; o++)//Ciklus: 2
                    {
                       
                        h2 = o + 1;
                        repeate2 = false;
                        bc = 0;


                        do//Ciklus: 3
                        {
                            bc = -1;//Jó helyen Van

                            do//Ciklus: 4
                            {

                                h2--;


                                bc++;

                                handler2 = GetClasssTeachers(tanarok, new Coord { DayIndex = (byte)dayIndex, OraIndex = (byte)i, osztalyIndex = (byte)h2 }, osztalyok[h2]);


                                if (handler2.Count == 0)
                                {
                                    repeate2 = true;
                                    change = false;
                                }
                                else
                                {

                                    handler2 = GetTeachers(handler2, osztalyok, new Coord { osztalyIndex = (byte)h2, OraIndex = i.ToByte(), DayIndex = (byte)dayIndex });



                                    //Ide kell egy metódus ami fontossági sorrendbe rakja a tanárok óráit


                                    handler2 = LessonPrioriti(osztalyok[h2], handler2, new Coord { osztalyIndex = (byte)h2, OraIndex = (byte)i, DayIndex = (byte)dayIndex });
                                    //Console.WriteLine(handler2.Count);
                                    Console.WriteLine("cha = {0}", cha);


                                    if (handler2.Count == 0)
                                    {
                                        repeate2 = true;
                                        change = false;
                                    }
                                    else
                                    {
                                        //kellet ismételni vagyis a vissza fejtett tanár indexhez hozzá kell adni egyet ha lehet
                                        if (repeate2 == true)
                                        {

                                            repeate2 = false;
                                            if (handler2.Count == 1)
                                            {
                                                //Itt azért kell ismételni mert itt több mint egy tanár kell

                                                repeate2 = true;
                                                //Ezt itt át kell nézni!!!!

                                                if (fbc)
                                                {
                                                    h2 = 0;
                                                    cha++;
                                                    needChange = true;


                                                    i -= cha;

                                                    if (i < 0)
                                                    {
                                                        i = 0;

                                                    }

                                                }
                                            }
                                            else
                                            {

                                                if (handler2.Count > 1)
                                                {



                                                    //Ha nincs bene akkor oda rakjuk az elsőt
                                                    if (handler2.GetNames().Contains(osztalyok[h2].OraTipusok[osztalyok[h2].Napok[dayIndex].Orak[i].OraIndex].Tanar.Nev) == false)
                                                    {
                                                        //Ide is rakhatunk prioritást

                                                        osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = handler2[0].OraTypeIndexes[h2][0] };
                                                        change = true;

                                                        cha = 0;

                                                        bcindex = h2;
                                                    }
                                                    else //Ha benne van akkor
                                                    {



                                                        //Itt kell vissza fejteni
                                                        //Vagyis kell egy metódus ami vissza adja a kövi tanárt

                                                        htanar = GetNextTeacher(osztalyok[h2].OraTipusok[osztalyok[o].Napok[dayIndex].Orak[i].OraIndex].Tanar, handler2);
                                                        //Ha nincs kövi tanár 
                                                        if (htanar.OraTypeIndexes == null)
                                                        {

                                                            //Ezt itt át kell nézni!!!!
                                                            repeate2 = true;

                                                            needChange = true;

                                                            if (fbc)
                                                            {
                                                                //Console.ReadLine();
                                                                h2 = 0;
                                                                if (i - cha >= 0)
                                                                {
                                                                    i -= cha;

                                                                    cha++;
                                                                    //needChange = true;

                                                                }

                                                            }


                                                            change = false;
                                                        }
                                                        else
                                                        {

                                                            osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = htanar.OraTypeIndexes[h2][0] };
                                                            bcindex = h2;
                                                            change = true;

                                                            if (fbc == false)
                                                            {
                                                                cha = 0;
                                                            }

                                                            //cha = i;

                                                        }
                                                    }
                                                }
                                            }

                                        }
                                        else//Nincs repeat
                                        {

                                            if (handler2.Count == 1)
                                            {//Itt csak egy tanár lehet

                                                //Ide kell egy funkció ami fontossági sorrendbe rakja az adott tanárnak az óráit

                                                osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = LessonPrioriti(osztalyok[h2], handler2, new Coord { osztalyIndex = (byte)h2, DayIndex = (byte)dayIndex, OraIndex = (byte)i })[0].OraTypeIndexes[h2][0] };



                                                change = true;
                                                bcindex = h2;
                                            }
                                            else//Nem egy tanár van
                                            {
                                                if (handler2.Count > 1)//Több mint egy tanár van
                                                {


                                                    //Ide kell egy metódust ami kiválasztja a legjobban kellő tanárt
                                                    if (needChange)
                                                    {
                                                        needChange = false;


                                                        if (change == true) //Ez azért kell mert ha (balrább )volt változás akkor nem a kövit kell nézni
                                                        {
                                                            osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = handler2[0].OraTypeIndexes[h2][0] };

                                                        }
                                                        else
                                                        {
                                                            TableElements.Tanar asd = new TableElements.Tanar();


                                                            asd = GetNextTeacher(osztalyok[h2].OraTipusok[osztalyok[h2].Napok[dayIndex].Orak[i].OraIndex].Tanar, handler2);

                                                            if (asd.OraTypeIndexes == null)
                                                            {
                                                                repeate2 = true;

                                                                if (i == 0)
                                                                {
                                                                    //Itt kell vissza menni egy napot

                                                                    if (dayIndex - 1 >= 0)
                                                                    {
                                                                        dayIndex--;
                                                                        needChange = true;
                                                                        i = handler.Min() - 1;
                                                                        h2 = 0;
                                                                    }

                                                                }

                                                                change = false;
                                                            }
                                                            else
                                                            {
                                                                osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = asd.OraTypeIndexes[h2][0] };
                                                                change = true;

                                                            }
                                                        }




                                                    }
                                                    else
                                                    {

                                                        if (cha != 0)
                                                        {

                                                            if (change == true) //Ez azért kell mert ha (balrább )volt változás akkor nem a kövit kell nézni
                                                            {
                                                                osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = handler2[0].OraTypeIndexes[h2][0] };

                                                            }
                                                            else
                                                            {


                                                                TableElements.Tanar asd = new TableElements.Tanar();


                                                                asd = GetNextTeacher(osztalyok[h2].OraTipusok[osztalyok[h2].Napok[dayIndex].Orak[i].OraIndex].Tanar, handler2);

                                                                if (asd.OraTypeIndexes == null)
                                                                {
                                                                    repeate2 = true;
                                                                    change = false;
                                                                }
                                                                else
                                                                {
                                                                    osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = asd.OraTypeIndexes[h2][0] };
                                                                    change = true;

                                                                    if (fbc == false)//Ez itt talán jó helyen van
                                                                    {
                                                                        cha = 0;
                                                                    }
                                                                }
                                                            }


                                                        }
                                                        else//cha = 0 vagyis nincs full backtrack
                                                        {
                                                            //Console.ReadLine();
                                                            osztalyok[h2].Napok[dayIndex].Orak[i] = new TableElements.Ora { OraIndex = handler2[0].OraTypeIndexes[h2][0] };
                                                            change = true;
                                                        }


                                                        bcindex = h2;

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }


                                //Console.Clear();
                                Console.SetCursorPosition(0, 0);
                                //Console.WriteLine(ErrorIdentity.ToString());

                                osztalyok.ShowSpecificDayT(new Gen.Coord { osztalyIndex = (byte)h2, DayIndex = (byte)dayIndex, OraIndex = (byte)i }, (byte)dayIndex);
                                //osztalyok.ShowFullTabel();
                                Console.WriteLine("{0}              ", string.Join(',', handler2.GetNames()));
                                //Console.WriteLine(h2);
                                //
                                //System.Threading.Thread.Sleep(100);
                                //Console.ReadLine();




                            } while (h2 > 0 && repeate2);


                            repeate2 = false;

                            if (bc != 0 && fbc == false)
                            {
                                fbc = true;
                            }

                            if (fbc)
                            {
                                h2 += 2;

                            }

                        }
                        while (h2 <= osztalyok.Count && fbc);
                        fbc = false;

                    }
                }
            }



            



            return osztalyok;
        }
    }

    public static class BuildTables
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="osztaly"></param>
        /// <param name="napok">Ennyi napos a munkahét</param>
        /// <returns></returns>
        public static List<TableElements.Osztaly> Build(this List<TableElements.Osztaly> osztaly, byte napok = 5)
        {

            List<TableElements.Ora> nap = new List<TableElements.Ora>();
            List<TableElements.Nap> het = new List<TableElements.Nap>();

            TableElements.Osztaly handler = new TableElements.Osztaly();

            for (int o = 0; o < osztaly.Count; o++)
            {
                nap = new List<TableElements.Ora>();
                het = new List<TableElements.Nap>();

                //Itt csinál egy napot
                for (int n = 0; n < osztaly[o].NapiMaxOrak; n++)
                {
                    nap.Add(new TableElements.Ora { OraIndex = 0, TanteremSzam = osztaly[o].TanteremSzama });
                }

                for (int i = 0; i < napok; i++)
                {
                    het.Add(new TableElements.Nap { Orak = nap, NapNev = TableElements.Napok[i] });
                }

                handler = osztaly[o];

                handler.Napok = het;

                osztaly[o] = handler;


            }


            return osztaly;
        }


        public static int NameIndexOf(this List<TableElements.Tanar> tanarok, string name)
        {
            int i = 0;

            while (i < tanarok.Count && tanarok[i].Nev != name)
            {
                i++;
            }

            return i;
        }

        /// <summary>
        /// Hozzá rendeli a tanárokhoz azokat az órákat amiket tanít
        /// </summary>
        /// <param name="tanarok"></param>
        /// <param name="oratipusok"></param>
        /// <returns></returns>
        public static List<TableElements.Tanar> Build(this List<TableElements.Tanar> tanarok, List<TableElements.Osztaly> osztalyok)
        {
            #region Build Tanarok typindex struct
            TableElements.Tanar handler = new TableElements.Tanar();
            for (int i = 0; i < tanarok.Count; i++)
            {
                handler = tanarok[i];

                handler.OraTypeIndexes = new List<List<byte>>();

                tanarok[i] = handler;
            }

            for (int i = 0; i < tanarok.Count; i++)
            {
                for (int a = 0; a < osztalyok.Count; a++)
                {
                    tanarok[i].OraTypeIndexes.Add(new List<byte>());
                }
            }

            #endregion



            for (int i = 0; i < osztalyok.Count; i++)
            {
                for (int t = 0; t < osztalyok[i].OraTipusok.Count; t++)
                {

                    tanarok[tanarok.NameIndexOf(osztalyok[i].OraTipusok[t].Tanar.Nev)].OraTypeIndexes[i].Add((byte)t);
                }
            }

            return tanarok;
        }
    }
    public static class TableElements
    {
        public static string[] Napok = new string[] { "Hétfő", "Kedd", "Szerda", "Csütörtök", "Péntek", "Szombat", "Vasárnap" };


        public struct Tanterem
        {
            public int FeroHely;
            public int TanteremSzam;


            public byte TypeIndex;
        }

        public struct Interval
        {
            /// <summary>
            /// true vagyis egésznap részt tud venni
            /// </summary>
            public bool FullDay;

            /// <summary>
            /// Ettől az órától tud részt venni ezt beleértve
            /// </summary>
            public byte StarterOra;

            /// <summary>
            /// Ez az utolsó óra amin részt tud venni
            /// </summary>
            public byte Endora;
        }



        public struct Tanar
        {



            /// <summary>
            /// A hét napjai szerint van
            /// </summary>
            public List<Interval> Reszvetel;


            /// <summary>
            /// 1d osztalyok; 2d az osztályhoz tartozó órafajták
            /// </summary>
            public List<List<byte>> OraTypeIndexes;

            public string Nev;
        }

        public struct Ora
        {

            //public string Nev;
            public byte OraIndex;
            /// <summary>
            /// pl: A vagy B csoport
            /// </summary>
            public string Csoport;
            public int TanteremSzam;


        }

        public struct OraTipus
        {
            /// <summary>
            /// A tanár  aki tartja az órát
            /// </summary>
            public Tanar Tanar;

            /// <summary>
            /// lehet-e egymás után 2 ilyen óra
            /// </summary>
            public bool LeheteDupla;

            /// <summary>
            /// Egy héten ennyi ora kell
            /// </summary>
            public byte HetiElfordulas;
            public string Oraneve;

            /// <summary>
            /// true = az A és B csoportnak ez az óra közös
            /// </summary>
            public bool CsoportFuggo;

            /// <summary>
            /// Tanterem típus lista
            /// </summary>
            public List<byte> TteremTypeIndexes;
        }
        public struct Nap
        {
            public string NapNev;
            public List<Ora> Orak;

        }

        public struct Osztaly
        {
            public string Nev;
            public string OsztalyFonok;
            public int TanteremSzama;
            public byte HetiOrakSzama;
            public byte NapiMaxOrak;
            //public string[] Csoportok;

            public byte Letszam;
            public List<OraTipus> OraTipusok;
            public List<Nap> Napok;

        }
    }



    public static class Show
    {
        public static void ShowData(this List<TableElements.Tanar> tanarok)
        {
            for (int i = 0; i < tanarok.Count; i++)
            {
                Console.WriteLine("[{0}]", tanarok[i].Nev);

                for (int a = 0; a < tanarok[i].OraTypeIndexes.Count; a++)
                {
                    Console.WriteLine("|  |-[{0}]", a);
                    for (int b = 0; b < tanarok[i].OraTypeIndexes[a].Count; b++)
                    {
                        Console.WriteLine("|  |  |-[{0}]", tanarok[i].OraTypeIndexes[a][b]);
                    }
                }
            }


        }

        public static void ShowTanarTanitas(this List<TableElements.Tanar> tanarok, List<List<byte>> oragyak)
        {
            for (int i = 0; i < tanarok.Count; i++)
            {
                Console.WriteLine("[{0}]", tanarok[i].Nev);
                Console.WriteLine("|   |-[{0}]", oragyak[i].Summ());

            }
        }
        public static string GetData(this List<TableElements.Tanar> tanarok)
        {
            string handler = string.Empty;


            for (int i = 0; i < tanarok.Count; i++)
            {
                //Console.WriteLine("[{0}]", tanarok[i].Nev);
                handler += string.Format("[{0}]\n", tanarok[i].Nev);

                for (int a = 0; a < tanarok[i].OraTypeIndexes.Count; a++)
                {
                    //Console.WriteLine("|  |-[{0}]", a);
                    handler += string.Format("|  |-[{0}]\n", a);

                    for (int b = 0; b < tanarok[i].OraTypeIndexes[a].Count; b++)
                    {
                        //Console.WriteLine("|  |  |-[{0}]", tanarok[i].OraTypeIndexes[a][b]);
                        handler += string.Format("|  |  |-[{0}]\n", tanarok[i].OraTypeIndexes[a][b]);

                    }
                }
            }

            return handler;
        }

        public static void ShowSpecificDay(this List<TableElements.Osztaly> osztaly, byte chspace = 8, byte napindex = 0)
        {
            string handler = string.Empty;
            for (int i = 0; i < osztaly.Count; i++)
            {
                Console.WriteLine(osztaly[i].Nev);

                Console.WriteLine("|--[{0}]", TableElements.Napok[napindex]);
                Console.Write("   |[A] ");

                for (int a = 0; a < osztaly[i].Napok[napindex].Orak.Count; a++)
                {
                    //Console.Write("{0}|", osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Oraneve);
                    handler = osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Oraneve;
                    for (int c = 0; c < chspace; c++)
                    {
                        if (c < handler.Length)
                        {
                            Console.Write(handler[c]);

                        }
                        else
                        {
                            Console.Write(" ");
                        }
                    }
                    Console.Write("|");


                }
                Console.WriteLine();
            }
        }



        public static void ShowSpecificDay(this List<TableElements.Osztaly> osztaly, Gen.Coord coord, byte napindex = 0, byte chspace = 6)
        {
            string handler = string.Empty;
            for (int i = 0; i < osztaly.Count; i++)
            {
                Console.WriteLine(osztaly[i].Nev);

                Console.WriteLine("|--[{0}]", TableElements.Napok[napindex]);
                Console.Write("   |[A] ");

                for (int a = 0; a < osztaly[i].Napok[napindex].Orak.Count; a++)
                {

                    if (coord.osztalyIndex == i && coord.DayIndex == napindex && coord.OraIndex == a)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }


                    //Console.Write("{0}|", osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Oraneve);
                    handler = osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Oraneve;
                    for (int c = 0; c < chspace; c++)
                    {
                        if (c < handler.Length)
                        {
                            Console.Write(handler[c]);
                        }
                        else
                        {
                            Console.Write(" ");
                        }

                    }
                    Console.Write("|");

                }
                Console.WriteLine();
            }
        }

        public static void ShowSpecificDayT(this List<TableElements.Osztaly> osztaly, Gen.Coord coord, byte napindex = 0, byte chSpace = 6)
        {
            string handler = string.Empty;
            for (int i = 0; i < osztaly.Count; i++)
            {
                Console.WriteLine(osztaly[i].Nev);

                Console.WriteLine("|--[{0}]", TableElements.Napok[napindex]);
                Console.Write("   |[A] ");

                for (int a = 0; a < osztaly[i].Napok[napindex].Orak.Count; a++)
                {

                    if (coord.osztalyIndex == i && coord.DayIndex == napindex && coord.OraIndex == a)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }


                    //Console.Write("{0}|", osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Tanar.Nev);
                    handler = osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Tanar.Nev;
                    for (int c = 0; c < chSpace; c++)
                    {
                        if (c < handler.Length)
                        {
                            Console.Write(handler[c]);
                        }
                        else
                        {
                            Console.Write(" ");
                        }
                    }
                    Console.Write("|");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;

                }
                Console.WriteLine();
            }
        }


        public static void ShowSpecificDayT(this List<TableElements.Osztaly> osztaly, byte napindex = 0)
        {
            for (int i = 0; i < osztaly.Count; i++)
            {
                Console.WriteLine(osztaly[i].Nev);

                Console.WriteLine("|--[{0}]", TableElements.Napok[napindex]);
                Console.Write("   |[A] ");

                for (int a = 0; a < osztaly[i].Napok[napindex].Orak.Count; a++)
                {
                    Console.Write("{0}|", osztaly[i].OraTipusok[osztaly[i].Napok[napindex].Orak[a].OraIndex].Tanar.Nev);


                }
                Console.WriteLine();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="osztaly"></param>
        /// <param name="index">true show just the index</param>
        public static void ShowFullTabel(this List<TableElements.Osztaly> osztaly, bool index = false)
        {

            for (int o = 0; o < osztaly.Count; o++)
            {
                Console.WriteLine(osztaly[o].Nev);

                for (int n = 0; n < osztaly[o].Napok.Count; n++)
                {
                    Console.WriteLine("|--[{0}]", TableElements.Napok[n]);

                    Console.Write("|  |--[A] ");
                    for (int i = 0; i < osztaly[o].Napok[n].Orak.Count; i++)
                    {
                        if (index)
                        {
                            Console.Write("{0}|", osztaly[o].Napok[n].Orak[i].OraIndex);
                        }
                        else
                        {
                            Console.Write("{0}|", osztaly[o].OraTipusok[osztaly[o].Napok[n].Orak[i].OraIndex].Oraneve);
                        }

                    }

                    Console.WriteLine("|");
                }
                Console.WriteLine();
            }

        }



        public static void ShowFullTabel(this List<TableElements.Osztaly> osztaly, byte classIndex, bool index = false)
        {

            int o = classIndex;


            Console.WriteLine(osztaly[o].Nev);

            for (int n = 0; n < osztaly[o].Napok.Count; n++)
            {
                Console.WriteLine("|--[{0}]", TableElements.Napok[n]);

                Console.Write("|  |--[A] ");
                for (int i = 0; i < osztaly[o].Napok[n].Orak.Count; i++)
                {
                    if (index)
                    {
                        Console.Write("{0}|", osztaly[o].Napok[n].Orak[i].OraIndex);
                    }
                    else
                    {
                        Console.Write("{0}|", osztaly[o].OraTipusok[osztaly[o].Napok[n].Orak[i].OraIndex].Oraneve);
                    }

                }

                Console.WriteLine();
                Console.WriteLine("|");
            }
            Console.WriteLine();


        }

        public static void ShowLessonStruct(this List<TableElements.Tanar> tanarok, List<TableElements.Osztaly> osztalyok)
        {
            for (int i = 0; i < tanarok.Count; i++)
            {
                Console.WriteLine("|--[{0}]", tanarok[i].Nev);

                for (int a = 0; a < tanarok[i].OraTypeIndexes.Count; a++)
                {
                    Console.WriteLine("|  |");
                    Console.WriteLine("|  |--[{0}]", osztalyok[a].Nev);

                    Console.Write("|  |  |-->");
                    for (int b = 0; b < tanarok[i].OraTypeIndexes[a].Count; b++)
                    {
                        Console.Write("{0};", osztalyok[a].OraTipusok[tanarok[i].OraTypeIndexes[a][b]].Oraneve);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            #region Database
            List<TableElements.Osztaly> Osztalyok = new List<TableElements.Osztaly>();
            List<TableElements.OraTipus> orafajtak = new List<TableElements.OraTipus>();
            List<TableElements.Tanar> Tanarok = new List<TableElements.Tanar>();



            List<TableElements.Interval> time = new List<TableElements.Interval>();
            time.Add(new TableElements.Interval { FullDay = true });
            time.Add(new TableElements.Interval { FullDay = true });
            time.Add(new TableElements.Interval { FullDay = true });
            time.Add(new TableElements.Interval { FullDay = true });
            time.Add(new TableElements.Interval { FullDay = true });


            //tanterem szerint kell érteni a típust pl infó óra csak infó típusó terembe lehet tartani
            List<string> oratipusok = new List<string>();
            oratipusok.Add("info");
            oratipusok.Add("altalanos");


            Tanarok.Add(new TableElements.Tanar { Nev = "Sanyi", Reszvetel = time });
            Tanarok.Add(new TableElements.Tanar { Nev = "Pityu", Reszvetel = time });
            Tanarok.Add(new TableElements.Tanar { Nev = "Feri", Reszvetel = time });
            Tanarok.Add(new TableElements.Tanar { Nev = "Enikő", Reszvetel = time });
            Tanarok.Add(new TableElements.Tanar { Nev = "Jani", Reszvetel = time });



            orafajtak.Add(new TableElements.OraTipus { Oraneve = "Angol", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 1 }, Tanar = Tanarok[0], LeheteDupla = false, CsoportFuggo = false });
            orafajtak.Add(new TableElements.OraTipus { Oraneve = "Magyar", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 1 }, Tanar = Tanarok[1], LeheteDupla = false, CsoportFuggo = false });
            orafajtak.Add(new TableElements.OraTipus { Oraneve = "Info", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 0 }, Tanar = Tanarok[2], LeheteDupla = false, CsoportFuggo = false });
            orafajtak.Add(new TableElements.OraTipus { Oraneve = "Tesi", HetiElfordulas = 5, TteremTypeIndexes = new List<byte>() { 0 }, Tanar = Tanarok[4], LeheteDupla = false, CsoportFuggo = false });




            List<TableElements.Tanterem> tantermek = new List<TableElements.Tanterem>();
            tantermek.Add(new TableElements.Tanterem { TanteremSzam = 202, FeroHely = 20, TypeIndex = 0 });
            tantermek.Add(new TableElements.Tanterem { TanteremSzam = 203, FeroHely = 23, TypeIndex = 1 });


            #region uj rész
            List<TableElements.OraTipus> ujorak = new List<TableElements.OraTipus>();


            ujorak.Add(new TableElements.OraTipus { Oraneve = "Angol", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 1 }, Tanar = Tanarok[4], LeheteDupla = false, CsoportFuggo = false });
            ujorak.Add(new TableElements.OraTipus { Oraneve = "Magyar", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 1 }, Tanar = Tanarok[2], LeheteDupla = false, CsoportFuggo = false });
            ujorak.Add(new TableElements.OraTipus { Oraneve = "Info", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 0 }, Tanar = Tanarok[1], LeheteDupla = false, CsoportFuggo = false });
            ujorak.Add(new TableElements.OraTipus { Oraneve = "Tesi", HetiElfordulas = 5, TteremTypeIndexes = new List<byte>() { 0 }, Tanar = Tanarok[3], LeheteDupla = false, CsoportFuggo = false });


            #endregion


            #region uj uj rész
            List<TableElements.OraTipus> ujorak2 = new List<TableElements.OraTipus>();


            ujorak2.Add(new TableElements.OraTipus { Oraneve = "Angol", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 1 }, Tanar = Tanarok[0], LeheteDupla = false, CsoportFuggo = false });
            ujorak2.Add(new TableElements.OraTipus { Oraneve = "Magyar", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 1 }, Tanar = Tanarok[1], LeheteDupla = false, CsoportFuggo = false });
            ujorak2.Add(new TableElements.OraTipus { Oraneve = "Info", HetiElfordulas = 10, TteremTypeIndexes = new List<byte>() { 0 }, Tanar = Tanarok[4], LeheteDupla = false, CsoportFuggo = false });
            ujorak2.Add(new TableElements.OraTipus { Oraneve = "Tesi", HetiElfordulas = 5, TteremTypeIndexes = new List<byte>() { 0 }, Tanar = Tanarok[2], LeheteDupla = false, CsoportFuggo = false });


            #endregion


            Osztalyok.Add(new TableElements.Osztaly { Nev = "9.Ny", Letszam = 10, TanteremSzama = 108, OraTipusok = orafajtak.ToArray().ToList(), OsztalyFonok = "Sanyi", HetiOrakSzama = 35, NapiMaxOrak = 7 });

            Osztalyok.Add(new TableElements.Osztaly { Nev = "9.B", Letszam = 10, TanteremSzama = 109, OraTipusok = ujorak.ToArray().ToList(), OsztalyFonok = "Sanyi", HetiOrakSzama = 35, NapiMaxOrak = 7 });

            Osztalyok.Add(new TableElements.Osztaly { Nev = "10.Ny", Letszam = 10, TanteremSzama = 110, OraTipusok = ujorak2.ToArray().ToList(), OsztalyFonok = "Sanyi", HetiOrakSzama = 35, NapiMaxOrak = 7 });

            Osztalyok.Add(new TableElements.Osztaly { Nev = "10.B", Letszam = 10, TanteremSzama = 111, OraTipusok = ujorak2.ToArray().ToList(), OsztalyFonok = "Sanyi", HetiOrakSzama = 35, NapiMaxOrak = 7 });

            #endregion

            Osztalyok.Build();
            Tanarok.Build(Osztalyok);

            //tanarok.ShowLessonStruct(Osztalyok);

            //Osztalyok.ShowFullTabel();

            //DateTime start = new DateTime();
            //start = DateTime.Now;
            //Osztalyok.GenOneDay(tanarok, 1).ShowSpecificDayT(1);


            //for (byte i = 0; i < 5; i++)
            //{
            //    Osztalyok.GenOneDay(tanarok, i);
            //}


            //DateTime start = new DateTime();
            //start = DateTime.Now;


            //for (byte i = 0; i < 5; i++)
            //{
            //    //Tanarok.ShowData();
            //    Osztalyok = Osztalyok.GenOneDay(Tanarok, i);
            //    //Osztalyok.ShowSpecificDay(i);
            //    //Tanarok.ShowData();
            //    //Console.WriteLine(i);
            //}

            Osztalyok = Osztalyok.GenOneWeek(Tanarok);

            //Console.WriteLine("##################################");
            //Osztalyok.ShowSpecificDay(1);
            //Osztalyok.ShowFullTabel();
            //Console.WriteLine("Számítási idő: {0}", DateTime.Now - start);
            Console.ReadLine();
        }
    }
}


