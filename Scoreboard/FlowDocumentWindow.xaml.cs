using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for FlowDocumentWindow.xaml
    /// </summary>
    public partial class FlowDocumentWindow : Window
    {
        private string _statistics;
        public string Statistics
        {
            get
            {
                return _statistics;
            }
            set
            {
                _statistics = value;
                DisplayStatistics();
            }
        }

        public FlowDocumentWindow()
        {
            InitializeComponent();
        }

        public static void ShowStatistics(Window owner, string statistics)
        {
            FlowDocumentWindow window = new FlowDocumentWindow
            {
                Owner = owner,
                Statistics = statistics
            };
            window.ShowDialog();
        }

        private void DisplayStatistics()
        {
            string[] lines = Statistics.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            StringBuilder documentStringBuilder = new StringBuilder();

            documentStringBuilder.AppendLine("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" Background=\"White\">");
            documentStringBuilder.AppendLine("<Table>");
            documentStringBuilder.AppendLine("<TableRowGroup Paragraph.TextAlignment = \"left\">");

            bool isBold = false;            
            foreach (string line in lines)
            {
                isBold = line.StartsWith("<b>");

                string workingLine = isBold ? line.Substring(3) : line;
                string fontWeight = isBold ? "FontWeight=\"Bold\"" : "FontWeight=\"Normal\"";

                documentStringBuilder.AppendLine("<TableRow " + fontWeight + ">");

                string[] columns = workingLine.Split('\t');
                if (columns.Length == 1)
                {
                    documentStringBuilder.AppendLine("<TableCell ColumnSpan=\"5\"><Paragraph>" + columns[0] + "</Paragraph></TableCell>");
                }
                else
                {
                    foreach (string column in columns)
                    {
                        documentStringBuilder.AppendLine("<TableCell><Paragraph>" + column + "</Paragraph></TableCell>");
                    }
                }

                documentStringBuilder.AppendLine("</TableRow>");                
            }
            documentStringBuilder.AppendLine("</TableRowGroup>");
            documentStringBuilder.AppendLine("</Table>");
            documentStringBuilder.AppendLine("</FlowDocument>");

            StringReader stringReader = new StringReader(documentStringBuilder.ToString());
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            FlowDocument document = (FlowDocument)XamlReader.Load(xmlReader);
            _flowDocumentScrollViewer.Document = document;            
        }

        private string RemoveMarkup(string text)
        {
            return text.Replace("<b>", String.Empty);
        }

        private void CopyToClipboardClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RemoveMarkup(Statistics));
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
