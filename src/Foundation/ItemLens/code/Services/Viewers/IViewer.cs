using Community.Foundation.ItemLens.Models;

namespace Community.Foundation.ItemLens.Services
{
    public interface IViewer
    {
        string GetHtml(LensInput input);
        
    }
}