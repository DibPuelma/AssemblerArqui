using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, string> hash;
        List<string> assembledLines;
        int contadorGlobal;
        public MainWindow()
        {
            InitializeComponent();
            createHash();            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name 
            dlg.DefaultExt = ".txt"; // Default file extension 
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension 

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                generateCPUInstructions(filename);
            }
            generateOutput()
        }
        private void generateCPUInstructions(string file)
        {
            assembledLines = new List<string>();
            addHeaderInstructions();
            StreamReader sr = new System.IO.StreamReader(file);
            string line = sr.ReadLine();
            if (line.Trim() == "DATA:")
            {
                readData(sr);
            }
            if (line.Trim() == "CODE:")
            {
                readCode(sr);
            }
            sr.Close();
        }
        private void readData(StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line.Trim() != "CODE:")
            {
                line = deleteComments(line);
                processDataString(line);
                line = sr.ReadLine();
            }
        }
        private void processDataString(string data)
        {
            //AL AGREGAR UNA INSTRUCCION ACORDARSE DE INCREMENTAR "contadorGlobal" EN 1
        }
        private void readCode(StreamReader sr)
        {
            while (sr.Peek() != -1)
            {
                processCodeString(sr.ReadLine());
            }

        }
        private void processCodeString(string code)
        {
            code = deleteComments(code);
            code.Trim();
            if (code.EndsWith(":")) //ES UN LABEL
            {
                //TODO: codigo para guardar en memoria el label
            }
            else // ES UNA INSTRUCCIÓN
            {
                int validador = 0;
                List<char> codeList = code.ToList<char>();
                codeList.RemoveAll(isWhiteSpace);
                string newCode = codeList.ToString();
                string[] verificador = newCode.Split(',');
                string litOrDir = "";
                if (verificador[1].StartsWith("(") && verificador[1].ElementAt(1).GetType().IsInstanceOfType(validador)) // El string despues de la coma es una dirección de memoria
                {
                    litOrDir = getBinaryValue(verificador[1]);
                    putInstructionInAssembler(verificador[0] + ",(DIR)", litOrDir); //AÑADIR LA DIRECCION, EL PROBLEMA ES QUE PUEDE SER BINARIO, DECIMAL O HEXA
                }
                else if (verificador[0].EndsWith(")") && verificador[0].ElementAt(verificador[0].Length-2).GetType().IsInstanceOfType(validador)) // El string antes de la coma es una dirección de memoria
                {
                    string[] direction = verificador[0].Split('(');
                    litOrDir = getBinaryValue("(" + direction[1]);
                    putInstructionInAssembler(direction[0] + "(DIR)," + verificador[1], litOrDir);
                }
                else if (verificador[1].ElementAt(0).GetType().IsInstanceOfType(validador)) // El string despues de la coma es un literal
                {
                    litOrDir = getBinaryValue("(" + verificador[1] + ")");
                    putInstructionInAssembler(verificador[0] + ",LIT", litOrDir);
                }
                else //Es instrucción cualquiera
                {
                    putInstructionInAssembler(newCode, null); //Mapea la instruccion con su opcode, lo concatena con Mem[... y además lo guarda en assembledLines
                }
            }
        }
        private string getBinaryValue(string number) // number es de la forma '(numero)'
        {
            if (number.ElementAt(number.Length - 2) == 'b') //es binario
            {
                return number.Substring(1, number.Length - 3);
            }
            else if (number.ElementAt(number.Length - 2) == 'h') //es hexa
            {
                return hexConverter(number.Substring(1, number.Length - 3));
            }
            else //decimal
            {
                if (number.ElementAt(number.Length - 2) == 'd')
                {
                    return decimalConverter(number.Substring(1, number.Length - 3));
                }
                else
                {
                    return decimalConverter(number.Substring(1, number.Length - 2));
                }
            }
        }
        private static bool isWhiteSpace(char c)
        {
            return c == ' ';
        }
        private string deleteComments(string commentedLine)
        {
            if(commentedLine.Contains("//")){
                return  commentedLine.Split('/')[0];
            }
            return commentedLine;

        }
        private void addHeaderInstructions()
        {
            assembledLines.Add("module ROM(address, instruction);");
            assembledLines.Add("");
            assembledLines.Add("input [7:0] address;");
            assembledLines.Add("output reg [24:0] instruction;");
            assembledLines.Add("");
            assembledLines.Add("reg [24:0] Mem[255:0];");
            assembledLines.Add("");
            assembledLines.Add("always 0 (address)");
            assembledLines.Add("begin");
        }
        private void addFooterInstructions(int lastInstructionNumber)
        {
            while(contadorGlobal <= 255)
            {
                assembledLines.Add("Mem[" + contadorGlobal + "] = 25'b0000000000000000000000000;");
                contadorGlobal++;
            }
            assembledLines.Add("");
            assembledLines.Add("instruction = Mem[address];");
            assembledLines.Add("end");
            assembledLines.Add("");
            assembledLines.Add("endmodule");
        }
        private void createHash()
        {
            hash = new Dictionary<string, string>();
            hash.Add("MOVA,B", "101110000");
            hash.Add("MOVB,A", "010011000");
            hash.Add("MOVA,LIT", "101100000");
            hash.Add("MOVB,LIT", "011100000");
            hash.Add("MOVA,DIR", "101101000");
            hash.Add("MOVB,DIR", "011101000");
            hash.Add("MOVDIR,A", "000011001");
            hash.Add("MOVDIR,B", "001110001");
            hash.Add("ADDA,B", "100010000");
            hash.Add("ADDB,A", "010010000");
            hash.Add("ADDA,LIT", "100000000");
            hash.Add("ADDB,LIT", "010000000");
            hash.Add("SUBA,B", "100010010");
            hash.Add("SUBB,A", "010010010");
            hash.Add("SUBA,LIT", "100000010");
            hash.Add("SUBB,LIT", "010000010");
            hash.Add("INCB", "011010000");
            hash.Add("JEQINS", "000010110");
            hash.Add("JMPINS", "000010100");
        }
        private void putInstructionInAssembler(string instruction, string literal) //instrucción dentro del hash y literal debe ser binario de 5 digitos, por ejemplo "10010" y se refiere a literal o direccion de memoria
        {
            if (literal == null)
            {
                literal = "00000";
            }
            string start = "Mem[" + contadorGlobal + "] = 25'b00000000";
            string result = "";
            hash.TryGetValue(instruction, out result);
            result = start + result + "000" + literal +";";
            assembledLines.Add(result);
        }
        private string hexConverter(string hexvalue)
        {
            return Convert.ToString(Convert.ToInt32(hexvalue, 16), 2);

        }
        private string decimalConverter(string dec)
        {
            return Convert.ToString(Int32.Parse(dec), 2);
        }
        private void generateOutput()
        {

        }
    }
}
