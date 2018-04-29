using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.Controllers
{
    public class SysMenuController : BaseController
    {
        private ISysMenuRepository menuRepository;
        
        public SysMenuController(ISysMenuRepository menuRepository)
        {
            this.menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        }

        #region Views

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View(menuRepository.GetSingle(id));
        }

        #endregion

        #region Methods

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var rows = menuRepository.GetHomeMenusByTreeView(m=>m.Activable && m.Visiable && m.ParentId == 0).OrderBy(m=>m.SortIndex).ToList();
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetTreeMenus(int parentId = 0)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var nodes = menuRepository.GetMenusByTreeView(m=>m.Activable && m.ParentId == 0).OrderBy(m => m.SortIndex).Select(m=>GetTreeMenus(m,parentId)).ToList();
                var rows = new[]
                {
                    new
                    {
                        text = " ���ڵ�",
                        icon = "fas fa-boxes",
                        tags = "0",
                        nodes,
                        state = new
                        {
                            selected = 0 == parentId
                        }
                    }
                };
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        private object GetTreeMenus(SysMenuViewModel viewModel, int parentId = 0)
        {
            if (viewModel.Children.Any())
            {
                return new
                {
                    text = " "+viewModel.MenuName,
                    icon = viewModel.MenuIcon,
                    tags = viewModel.Id.ToString(),
                    nodes = viewModel.Children.Select(GetTreeMenus),
                    state = new
                    {
                        expanded = false,
                        selected = viewModel.Id == parentId
                    }
                };
            }
            return new 
            {
                text = " "+viewModel.MenuName,
                icon = viewModel.MenuIcon,
                tags = viewModel.Id.ToString(),
                state = new
                {
                    selected = viewModel.Id == parentId
                }
            };
        }

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetMenusByPaged(int pageSize, int pageIndex)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var total = menuRepository.CountAsync(m => true).Result;
                var rows = menuRepository.GetByPagination(m => true, pageSize, pageIndex, true,
                    m => m.Id).ToList();
                return Json(PaginationResult.PagedResult(rows, total, pageSize, pageIndex));
            });
        }
        /// <summary>
        /// �½�
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost,ValidateAntiForgeryToken]
        public Task<IActionResult> Add(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("������֤ʧ��"));
                menuRepository.AddAsync(menu, true);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// �༭
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost]
        public Task<IActionResult> Edit(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("������֤ʧ��"));
                menuRepository.Edit(menu, true);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Delete(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                menuRepository.Delete(id);
                return Json(ExcutedResult.SuccessResult("�ɹ�ɾ��һ�����ݡ�"));
            });
        }

        /// <summary>
        /// ��ͣ��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Active(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var entity = menuRepository.GetSingle(id);
                entity.Activable = !entity.Activable;
                menuRepository.Update(entity, false, "Activable");
                return Json(ExcutedResult.SuccessResult(entity.Activable?"OK���ѳɹ����á�":"OK���ѳɹ�ͣ��"));
            });
        }
        /// <summary>
        /// �Ƿ������˵�����ʾ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Visualize(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var entity = menuRepository.GetSingle(id);
                entity.Visiable = !entity.Visiable;
                menuRepository.Update(entity, false, "Visiable");
                return Json(ExcutedResult.SuccessResult("�����ɹ�����ˢ�µ�ǰ��ҳ�������½���ϵͳ��"));
            });
        }

        #endregion
	}
}