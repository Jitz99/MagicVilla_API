using System.Collections.Generic;
using System.Reflection;
//using AspNetCore;
using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MagicVilla_Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;
        public VillaController(IVillaService villaService,IMapper mapper)
        {
            _villaService = villaService;
            _mapper = mapper;
        }
        public async Task<IActionResult> IndexVilla()
        {
            List<VillaDTO> list = new();
            var response = await _villaService.GetAllAsync<APIResponse>(HttpContext.Session.GetString(SD.SessionToken));
            if (response!=null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result));
            }
            return View(list);
        }
		public async Task<IActionResult> CreateVilla()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateVilla(VillaCreateDTO model)
		{
			if (ModelState.IsValid)
			{
				var response = await _villaService.CreateAsync<APIResponse>(model, HttpContext.Session.GetString(SD.SessionToken));
				if (response != null && response.IsSuccess)
				{
					TempData["success"] = "Villa Created Successfully";
					return RedirectToAction(nameof(IndexVilla));
				}
			}

            TempData["success"] = "error encountered";
            return View(model);
		}
		public async Task<IActionResult> UpdateVilla(int villaId)
		{
			var response = await _villaService.GetAsync<APIResponse>(villaId, HttpContext.Session.GetString(SD.SessionToken));
			if (response != null && response.IsSuccess)
			{
				VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
				return View(_mapper.Map<VillaUpdateDTO>(model));
			}
			return NotFound();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateVilla(VillaUpdateDTO model)
		{
			if (ModelState.IsValid)
			{
				var response = await _villaService.UpdateAsync<APIResponse>(model, HttpContext.Session.GetString(SD.SessionToken));
				if (response != null && response.IsSuccess)
				{
					TempData["success"] = "Villa Updated Successfully";
                    return RedirectToAction(nameof(IndexVilla));
				}
			}

            TempData["success"] = "error encountered";
			return View(model);
		}
		public async Task<IActionResult> DeleteVilla(int villaId)
		{
			var response = await _villaService.GetAsync<APIResponse>(villaId, HttpContext.Session.GetString(SD.SessionToken));
			if (response != null && response.IsSuccess)
			{
				VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
				return View(model);
			}
			return NotFound();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteVilla(VillaUpdateDTO model)
		{
			
			{
				var response = await _villaService.DeleteAsync<APIResponse>(model.Id, HttpContext.Session.GetString(SD.SessionToken));
				if (response != null && response.IsSuccess)
				{
					TempData["success"] = "Villa Deleted Successfully";
                    return RedirectToAction(nameof(IndexVilla));
				}
			}
            TempData["success"] = "error encountered";

            return View(model);
		}

		//public async Task<IActionResult> UpsertVilla(int? villaId)
		//{
		//	if (villaId == null || villaId == 0)
		//		return View();
		//	else
		//	{

		//		int id = (int)villaId;

		//		var response = await _villaService.GetAsync<APIResponse>(id);
		//		if (response != null && response.IsSuccess)
		//		{
		//			VillaDTO model = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
		//			return View(_mapper.Map<VillaUpdateDTO>(model));
		//		}
		//		return NotFound();

		//	}
		//}
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> UpsertVilla(VillaCreateDTO model)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		var response = await _villaService.CreateAsync<APIResponse>(model);
		//		if (response != null && response.IsSuccess)
		//		{
		//			return RedirectToAction(nameof(IndexVilla));
		//		}
		//	}

		//	return View(model);
		//}
	}
}
