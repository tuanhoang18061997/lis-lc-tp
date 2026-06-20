using Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Management.BL
{
    public class ZaloOAConfigBL
    {
        private readonly LABContext _context;

        public ZaloOAConfigBL(LABContext context)
        {
            _context = context;
        }

        public List<ZaloOAConfig> GetAll()
        {
            return _context.ZaloOAConfigs
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public ZaloOAConfig GetById(int id)
        {
            return _context.ZaloOAConfigs.FirstOrDefault(x => x.Id == id);
        }

        public bool Insert(ZaloOAConfig model, int createdBy)
        {
            model.CreatedOn = DateTime.Now;
            model.CreatedBy = createdBy;
            _context.ZaloOAConfigs.Add(model);
            return _context.SaveChanges() > 0;
        }

        public bool Update(ZaloOAConfig model, int updatedBy)
        {
            var entity = _context.ZaloOAConfigs.FirstOrDefault(x => x.Id == model.Id);
            if (entity == null) return false;

            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.HospitalId = model.HospitalId;
            entity.ApiUrl = model.ApiUrl;
            entity.Username = model.Username;
            entity.Password = model.Password;
            entity.ZaloOaId = model.ZaloOaId;
            entity.IsDefault = model.IsDefault;
            entity.IsActive = model.IsActive;
            entity.Notes = model.Notes;
            entity.UpdatedOn = DateTime.Now;
            entity.UpdatedBy = updatedBy;

            return _context.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var entity = _context.ZaloOAConfigs.FirstOrDefault(x => x.Id == id);
            if (entity == null) return false;

            _context.ZaloOAConfigs.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}