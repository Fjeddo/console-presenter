namespace ConsolePresenter;

public interface ISlideRenderer
{
    void Render(string content, int slideNumber, int totalSlides, string title);
}
