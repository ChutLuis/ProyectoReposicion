using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Reposición.Models;

namespace Reposición.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        
        public ActionResult Index()
        {
           
            string path2 = Path.Combine(Server.MapPath("~/microSQL/"), "microSQL.ini");
            Path_.Instance.a1 = path2;//Instancia del Path en el que se crea el archivo de lenguaje del programa ya que al usar otro tipo el programa no funciona
            TableFilePath_.Instance.a1 = Server.MapPath("~/microSQL/tablas/");
            TreeFilePath_.Instance.a1 = Server.MapPath("~/microSQL/arbolesb/");
            if (System.IO.File.Exists(Path_.Instance.a1))//Se valida si el archivo de lenguaje existe en el proyecto
            {
                ViewBag.Message="Existe el Archivo de lenguaje en el proyecto";
                Methods.FillReservatedWords(Path_.Instance.a1);
            }
            else
            {
                
                Methods.FillReservatedWords(Path_.Instance.a1);
                ViewBag.Message = "No existe el Archivo de lenguaje en el proyecto, se creo uno nuevo";
            }
            List<string> nameTable = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(Methods.TableFilePath);
            if (directory.GetFiles() != null)
            {
                FileInfo[] files = directory.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    StreamReader reader = new StreamReader(files[i].FullName);
                    string line = reader.ReadLine();
                    Methods.TablesList.Add(line);

                    string tableName = line;
                    string tableColumns = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        tableColumns += line + "|";
                    }
                    reader.Dispose();
                    reader.Close();
                    Methods.AddDataBaseItem(tableName, tableColumns.Remove(tableColumns.Length - 1));
                }
            }

            return View();
        }

        
        public ActionResult Table(string table)
        {

            ViewBag.Message = table;
            if (Methods.SetCurrentTable(table)!=false)
            {
                ViewBag.Message= "Existe La Tabla";
            }
            Methods.SetCurrentTable(table);
            Methods.rowsData();       
            return View();
        }

        //[HttpPost]
        public ActionResult openTable(string tableName)
        {
            ViewBag.Message = tableName;
            return View("Table");
        }
        /// <summary>
        /// En este metodo se carga un archivo para la creacion de la traduccion, 
        /// se decidio guardar el archivo en el proyecto para facilitar el acceso 
        /// para la creacion del archivo ini default
        /// </summary>
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            string _path = "";            
            try
            {
                string _FileName = Path.GetFileName(file.FileName);
                _path = Path.Combine(Server.MapPath("~/microSQL"), _FileName);                
                file.SaveAs(_path);//Se guarda el archivo para la lectura 
                

                if (file.FileName.Split('.')[1].ToUpper() == "INI" || file.FileName.Split('.')[1].ToUpper() == "CSV")
                {
                    Methods.FillReservatedWords(_path);
                    ViewBag.Message = Message.Instance.a1;                
                }
                else
                {
                      ViewBag.Message="No se seleccionó un archivo válido, el archivo de traducción no tiene un formato INI o CSV válido.";
                }
            }
            catch(Exception e)
            {
                ViewBag.Message = "The exception is "+ e;

            }

            return View("Index");
        }
        
        //Methodo para la lectura del codigo sql 
        [HttpPost]
        public ActionResult syntaxSQL(string code)
        {
            Methods.ReviewInstructios(code);
            if (Message.Instance.a1 == "Listo")
            {
                Methods.OperateInstructions(code);
            }
            ViewBag.Message = Message.Instance.a1;
            return View();
        }
        
        // POST: Home/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult selectTable()
        {
            ViewBag.Title = Methods.CurrentTable.TableName;
            return View();
        }

        public ActionResult TreeView()
        {

            return View();
        }

        [HttpPost]
        public ActionResult exportCSV(HttpPostedFileBase file)
        {
            string _path = "";
            try
            {
                string _FileName = Path.GetFileName(file.FileName);
                _path = Path.Combine(Server.MapPath("~/microSQL"), _FileName);
                file.SaveAs(_path);//Se guarda el archivo para la lectura 


                if (file.FileName.Split('.')[1].ToUpper() == "CSV")
                {
                    Methods.ExportCSV(_path);
                }
                else
                {
                    ViewBag.Message = "No se seleccionó un archivo válido, el archivo de traducción no tiene un formato INI o CSV válido.";
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "The exception is " + e;

            }
            return View("Table");
        }
        // POST: Home/Delete/5

        public ActionResult viewSQL()
        {
            return View("syntaxSQL");
        }
    }
}
