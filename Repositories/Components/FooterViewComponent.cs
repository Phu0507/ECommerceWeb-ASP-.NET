using Microsoft.AspNetCore.Mvc;

namespace Shopping_web.Repositories.Components
{
    public class FooterViewComponent : ViewComponent
    {
        public readonly DataContext _dataContext;
        public FooterViewComponent(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contact = _dataContext.Contacts.FirstOrDefault();
            return View(contact);
        }
    }
}
