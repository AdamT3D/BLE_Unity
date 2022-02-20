using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PageReader : MonoBehaviour
{
    public ArduinoHM10Test BLEInput;

    public Text currentPageOutput;

    public PlayableDirector director;
    public PlayableAsset page1;
    public PlayableAsset page2;
    public PlayableAsset page3;
    public PlayableAsset page4;
    public PlayableAsset page5;
    public PlayableAsset page6;
    public PlayableAsset page7;

    //private string expectedFormatForReferenceAndTesting = "Pages:{0001000}";
    private string prefix = "{";
    private string suffix = "}";
    private int NUMBEROFPAGES = 7;
    private string lastInput = "";

    private BookData book;

    private void Start()
    {
        //initialise book and its size using constant above
        book = new BookData(NUMBEROFPAGES);
    }

    void Update()
    {
        //check if input has been updated since last time, do not continue if it has not
        if(lastInput == BLEInput.inputText)
        {
            return;
        }
        lastInput = BLEInput.inputText;

        //parse new input into existing book, only continue if it was succesful
        bool parseSuccesful = book.TryParsePages(BLEInput.inputText, prefix, suffix);
        if (parseSuccesful)
        {
            book.UpdateBookStatus();
        }
        else
        {
            if (BLEInput.inputText.Contains(prefix) && BLEInput.inputText.Contains(suffix))
            {
                int pFrom = BLEInput.inputText.IndexOf(prefix) + prefix.Length;
                int pTo = BLEInput.inputText.LastIndexOf(suffix);
                string result = BLEInput.inputText.Substring(pFrom, pTo - pFrom);

                currentPageOutput.text =  $"'{result}'={result.Length}not{book.pages.Length}";  //ISSUE HERE WHEN BUILT
            }
            else
            {
                currentPageOutput.text = "Parse Unsuccesful, Unexpected input from bluetooth: " + BLEInput.inputText;
            }
                
            return;
        }

        //perform logic based on the updated book data
        if (book.isClosed)
        {
            currentPageOutput.text = "Book Is Closed";
        }
        else if (book.inTransition)
        {
            currentPageOutput.text = "A page is being turned";
        }
        else
        {
            currentPageOutput.text = "Page: " + book.currentPage.ToString();
            //HARDCODED TO 7 PAGES
            if (book.currentPage == 1)
            {
                director.playableAsset = page1;
                director.Play();
            }
            else if (book.currentPage == 2)
            {
                director.playableAsset = page2;
                director.Play();
            }
            else if (book.currentPage == 3)
            {
                director.playableAsset = page3;
                director.Play();
            }
            else if (book.currentPage == 4)
            {
                director.playableAsset = page4;
                director.Play();
            }
            else if (book.currentPage == 5)
            {
                director.playableAsset = page5;
                director.Play();
            }
            else if (book.currentPage == 6)
            {
                director.playableAsset = page6;
                director.Play();
            }
            else if (book.currentPage == 7)
            {
                director.playableAsset = page7;
                director.Play();
            }
        }

    }
}


public class BookData
{
    public (int PageNumber, int Status)[] pages; //tuple
    public int currentPage;
    public bool inTransition;
    public bool isClosed;

    public BookData(int numberOfPages)
    {
        pages = new (int Page, int Value)[numberOfPages];
        currentPage = 0; //default?
        inTransition = false;
        isClosed = true;
    }

    public bool TryParsePages(string input, string prefix, string suffix)
    {
        if (input.Contains(prefix) && input.Contains(suffix))
        {
            //remove braces used for message verification
            int pFrom = input.IndexOf(prefix) + prefix.Length;
            int pTo = input.LastIndexOf(suffix);
            string result = input.Substring(pFrom, pTo - pFrom);

            if (result.Length != pages.Length)
            {
                Debug.LogError("incorrect number of inputs from bluetooth: " + result.Length + "instead of " + pages.Length + " in " + result);
                return false;
            }

            for (int i = 0; i < pages.Length; i++)
            {
                int pageNumber = i + 1;
                int pageStatus = int.Parse(result[i].ToString());

                this.pages[i] = (pageNumber, pageStatus);
                Debug.Log($"page number {this.pages[i].PageNumber} has value: {this.pages[i].Status}");
            }
            return true;
        }
        else
        {
            Debug.LogError("Unexpected input from bluetooth: " + input);
            return false;
        }
    }

    public void UpdateBookStatus()
    {
        //gather info from pages
        int pagesClosed = 0;
        List<int> pagesOpen = new List<int>();
        foreach (var page in pages)
        {
            //INVERTED FFOR TESTIN!!!
            //if (page.Status == 0)
            if (page.Status == 1)
            {
                //magnet on sensor
                pagesClosed++;
            }
            //else if (page.Status == 1)
            else if (page.Status == 0)
            {
                //no magnet
                pagesOpen.Add(page.PageNumber);
            }
            else
            {
                Debug.LogError("Page Value not 0 or 1: " + page.Status);
            }
        }

        //deduce info from data and update variables into Book Object
        if (pagesOpen.Count == 1)
        {
            currentPage = pagesOpen[0];
            inTransition = false;
            isClosed = false;
        }
        else if (pagesOpen.Count > 1)
        {
            currentPage = 0;
            inTransition = true;
            isClosed = false;
        }
        else if (pagesOpen.Count == 0)
        {
            currentPage = 0;
            inTransition = false;
            isClosed = true;
        }
    }
}

