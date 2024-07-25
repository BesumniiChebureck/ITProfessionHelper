using System.ServiceModel.Syndication;

namespace ITProfessionHelperBot;

internal class Vacancy
{
    internal Vacancy(SyndicationItem? item)
    {
        title = item.Title.Text;
        description = item.Summary.Text;
        company = description[9..(description.IndexOf("ищет") - 1)];
        publishDate = item.PublishDate.DateTime;
        link = item.Links.FirstOrDefault().Uri.ToString();
    }

    public string ToPrintableFormat()
    {
        return $"{Title}\n\n" +
               $"{Description}\n\n" +
               $"{Company}\n\n" +
               $"{PublishDate.PrintableFormat()}";
    }

    private string title;
    private string description;
    private string company;
    private DateTime publishDate;
    private string link;
    
    internal string Title
    {
        get { return title; }
    }
    
    internal string Description
    {
        get
        {
            int indextToCut = description.IndexOf("). ");
            string result = description;
                
            if (indextToCut != -1)
            {
                result = result[(indextToCut + 3)..];
            }
            else
            {
                indextToCut = result.IndexOf(". П");
                    
                if (indextToCut != -1)
                {
                    result = result[(indextToCut + 2)..];
                }
            }

            return result;
        }
    }

    internal string Company
    {
        get { return company; }
    }

    internal DateTime PublishDate
    {
        get { return publishDate; }
    }

    internal string Link
    {
        get { return link; }
    }
}