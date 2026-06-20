using Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Management.BL
{
    public class ZaloOATemplateBL
    {
        private readonly LABContext _context;

        public ZaloOATemplateBL(LABContext context)
        {
            _context = context;
        }

        public List<ZaloOATemplate> GetAll()
        {
            return _context.ZaloOATemplates
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public ZaloOATemplate GetById(int id)
        {
            return _context.ZaloOATemplates.FirstOrDefault(x => x.Id == id);
        }

        public bool Insert(ZaloOATemplate model, int createdBy)
        {
            model.CreatedOn = DateTime.Now;
            model.CreatedBy = createdBy;
            _context.ZaloOATemplates.Add(model);
            return _context.SaveChanges() > 0;
        }

        public bool Update(ZaloOATemplate model, int updatedBy)
        {
            var entity = _context.ZaloOATemplates.FirstOrDefault(x => x.Id == model.Id);
            if (entity == null) return false;

            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.ZaloOAConfigId = model.ZaloOAConfigId;
            entity.HospitalId = model.HospitalId;
            entity.TemplateZaloId = model.TemplateZaloId;
            entity.Title = model.Title;
            entity.Content = model.Content;
            entity.IsDefault = model.IsDefault;
            entity.IsActive = model.IsActive;
            entity.Notes = model.Notes;
            entity.UpdatedOn = DateTime.Now;
            entity.UpdatedBy = updatedBy;

            return _context.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var entity = _context.ZaloOATemplates.FirstOrDefault(x => x.Id == id);
            if (entity == null) return false;

            _context.ZaloOATemplates.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}