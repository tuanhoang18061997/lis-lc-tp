using Management.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Management.BL
{
    public class ExternalFileBL
    {
        private readonly LABContext _db;

        public ExternalFileBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<ExternalFile>> GetList(bool all = false)
        {
            if (all)
            {
                return await _db.ExternalFiles
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();
            }

            return await _db.ExternalFiles
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<ExternalFile> Get(long id)
        {
            return await _db.ExternalFiles
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ExternalFile>> GetByPIdAndMaBenhAn(string pId, string maBenhAn, int moduleType, bool visibleOnly = false)
        {
            var query = _db.ExternalFiles
                .Where(x => x.ModuleType == moduleType
                         && x.IsDeleted == false);

            if (visibleOnly)
            {
                query = query.Where(x => x.IsVisibleToUser == true);
            }

            if (!string.IsNullOrWhiteSpace(pId))
            {
                query = query.Where(x => x.PId == pId);
            }

            if (!string.IsNullOrWhiteSpace(maBenhAn))
            {
                query = query.Where(x => x.MaBenhAn == maBenhAn);
            }

            return await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<ExternalFile>> GetVisibleFilesAsync(long? tablePatientId, string pId, string maBenhAn, int moduleType)
        {
            var baseQuery = _db.ExternalFiles
                .Where(x => x.ModuleType == moduleType
                         && x.IsDeleted == false
                         && x.IsVisibleToUser == true);

            // 1) Ưu tiên mạnh nhất: pId + maBenhAn
            if (!string.IsNullOrWhiteSpace(pId) && !string.IsNullOrWhiteSpace(maBenhAn))
            {
                var byPIdAndMaBenhAn = await baseQuery
                    .Where(x => x.PId == pId && x.MaBenhAn == maBenhAn)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byPIdAndMaBenhAn.Any())
                {
                    return byPIdAndMaBenhAn;
                }
            }

            // 2) Fallback theo MaBenhAn
            if (!string.IsNullOrWhiteSpace(maBenhAn))
            {
                var byMaBenhAn = await baseQuery
                    .Where(x => x.MaBenhAn == maBenhAn)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byMaBenhAn.Any())
                {
                    return byMaBenhAn;
                }
            }

            // 3) Fallback theo PId
            if (!string.IsNullOrWhiteSpace(pId))
            {
                var byPId = await baseQuery
                    .Where(x => x.PId == pId)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byPId.Any())
                {
                    return byPId;
                }
            }

            // 4) TablePatientId chỉ là fallback cuối cùng
            if (tablePatientId.HasValue && tablePatientId.Value > 0)
            {
                var byTablePatientId = await baseQuery
                    .Where(x => x.TablePatientId == tablePatientId.Value)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byTablePatientId.Any())
                {
                    return byTablePatientId;
                }
            }

            return new List<ExternalFile>();
        }

        public async Task<List<ExternalFile>> GetAllFilesAsync(long? tablePatientId, string pId, string maBenhAn, int moduleType)
        {
            var baseQuery = _db.ExternalFiles
                .Where(x => x.ModuleType == moduleType
                         && x.IsDeleted == false
                         );

            // 1) Ưu tiên mạnh nhất: pId + maBenhAn
            if (!string.IsNullOrWhiteSpace(pId) && !string.IsNullOrWhiteSpace(maBenhAn))
            {
                var byPIdAndMaBenhAn = await baseQuery
                    .Where(x => x.PId == pId && x.MaBenhAn == maBenhAn)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byPIdAndMaBenhAn.Any())
                {
                    return byPIdAndMaBenhAn;
                }
            }

            // 2) Fallback theo MaBenhAn
            if (!string.IsNullOrWhiteSpace(maBenhAn))
            {
                var byMaBenhAn = await baseQuery
                    .Where(x => x.MaBenhAn == maBenhAn)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byMaBenhAn.Any())
                {
                    return byMaBenhAn;
                }
            }

            // 3) Fallback theo PId
            if (!string.IsNullOrWhiteSpace(pId))
            {
                var byPId = await baseQuery
                    .Where(x => x.PId == pId)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byPId.Any())
                {
                    return byPId;
                }
            }

            // 4) TablePatientId chỉ là fallback cuối cùng
            if (tablePatientId.HasValue && tablePatientId.Value > 0)
            {
                var byTablePatientId = await baseQuery
                    .Where(x => x.TablePatientId == tablePatientId.Value)
                    .OrderByDescending(x => x.CreatedOn)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                if (byTablePatientId.Any())
                {
                    return byTablePatientId;
                }
            }

            return new List<ExternalFile>();
        }

        public async Task<bool> Save(ExternalFile model)
        {
            try
            {
                var obj = await _db.ExternalFiles
                    .Where(x => x.Id == model.Id)
                    .FirstOrDefaultAsync();

                if (obj == null)
                {
                    model.CreatedOn = model.CreatedOn == default ? DateTime.Now : model.CreatedOn;
                    await _db.ExternalFiles.AddAsync(model);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    obj.TablePatientId = model.TablePatientId;
                    obj.PId = model.PId;
                    obj.MaBenhAn = model.MaBenhAn;
                    obj.PatientName = model.PatientName;
                    obj.ModuleType = model.ModuleType;
                    obj.StoredFileName = model.StoredFileName;
                    obj.RelativePath = NormalizeRelativePath(model.RelativePath);
                    obj.IsVisibleToUser = model.IsVisibleToUser;
                    obj.IsDeleted = model.IsDeleted;
                    obj.CreatedBy = model.CreatedBy;

                    await _db.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateVisibility(long id, bool isVisibleToUser)
        {
            try
            {
                var obj = await _db.ExternalFiles
                    .Where(x => x.Id == id && x.IsDeleted == false)
                    .FirstOrDefaultAsync();

                if (obj == null) return false;

                obj.IsVisibleToUser = isVisibleToUser;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteByFileAsync(string storedFileName, string relativePath, int moduleType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storedFileName) || string.IsNullOrWhiteSpace(relativePath))
                    return false;

                relativePath = NormalizeRelativePath(relativePath);

                var rows = await _db.ExternalFiles
                    .Where(x => x.ModuleType == moduleType
                             && x.StoredFileName == storedFileName
                             && x.RelativePath == relativePath
                             && x.IsDeleted == false)
                    .ToListAsync();

                if (!rows.Any())
                    return false;

                foreach (var item in rows)
                {
                    item.IsDeleted = true;
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> SyncPhysicalFilesIfMissingAsync(
            IEnumerable<string> physicalFilePaths,
            string webRootPath,
            string pId,
            string maBenhAn,
            long tablePatientId,
            int moduleType,
            string patientName = null,
            long? createdBy = null,
            bool isVisibleToUser = true)
        {
            try
            {
                if (physicalFilePaths == null) return 0;
                if (string.IsNullOrWhiteSpace(webRootPath)) return 0;
                if (string.IsNullOrWhiteSpace(maBenhAn)) return 0;

                var files = physicalFilePaths
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(System.IO.File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!files.Any()) return 0;

                int insertedCount = 0;

                foreach (var physicalPath in files)
                {
                    var storedFileName = Path.GetFileName(physicalPath);
                    if (string.IsNullOrWhiteSpace(storedFileName)) continue;

                    var relativePath = ConvertPhysicalPathToRelativePath(physicalPath, webRootPath);
                    relativePath = NormalizeRelativePath(relativePath);
                    if (string.IsNullOrWhiteSpace(relativePath)) continue;

                    // check trùng theo ModuleType + StoredFileName + RelativePath
                    var existed = await _db.ExternalFiles.AnyAsync(x =>
                        x.IsDeleted == false
                        && x.ModuleType == moduleType
                        && x.StoredFileName == storedFileName
                        && x.RelativePath == relativePath);

                    if (existed) continue;

                    await _db.ExternalFiles.AddAsync(new ExternalFile
                    {
                        TablePatientId = tablePatientId,
                        PId = pId,
                        MaBenhAn = maBenhAn,
                        PatientName = patientName,
                        ModuleType = moduleType,
                        StoredFileName = storedFileName,
                        RelativePath = relativePath,
                        IsVisibleToUser = isVisibleToUser,
                        IsDeleted = false,
                        CreatedOn = DateTime.Now,
                        CreatedBy = createdBy
                    });

                    insertedCount++;
                }

                if (insertedCount > 0)
                {
                    await _db.SaveChangesAsync();
                }

                return insertedCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<long> EnsureFileSyncedAsync(
            string storedFileName,
            string relativePath,
            string pId,
            string maBenhAn,
            long tablePatientId,
            int moduleType,
            string patientName = null,
            long? createdBy = null,
            bool isVisibleToUser = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storedFileName)) return 0;
                if (string.IsNullOrWhiteSpace(relativePath)) return 0;
                if (string.IsNullOrWhiteSpace(maBenhAn)) return 0;

                relativePath = NormalizeRelativePath(relativePath);

                var existed = await _db.ExternalFiles
                    .Where(x => x.IsDeleted == false
                             && x.ModuleType == moduleType
                             && x.StoredFileName == storedFileName
                             && x.RelativePath == relativePath)
                    .FirstOrDefaultAsync();

                if (existed != null)
                {
                    bool changed = false;

                    if (string.IsNullOrWhiteSpace(existed.PId) && !string.IsNullOrWhiteSpace(pId))
                    {
                        existed.PId = pId;
                        changed = true;
                    }

                    if (string.IsNullOrWhiteSpace(existed.MaBenhAn) && !string.IsNullOrWhiteSpace(maBenhAn))
                    {
                        existed.MaBenhAn = maBenhAn;
                        changed = true;
                    }

                    if (string.IsNullOrWhiteSpace(existed.PatientName) && !string.IsNullOrWhiteSpace(patientName))
                    {
                        existed.PatientName = patientName;
                        changed = true;
                    }

                    if (existed.TablePatientId <= 0 && tablePatientId > 0)
                    {
                        existed.TablePatientId = tablePatientId;
                        changed = true;
                    }

                    if (changed)
                    {
                        await _db.SaveChangesAsync();
                    }

                    return existed.Id;
                }

                var entity = new ExternalFile
                {
                    TablePatientId = tablePatientId,
                    PId = pId,
                    MaBenhAn = maBenhAn,
                    PatientName = patientName,
                    ModuleType = moduleType,
                    StoredFileName = storedFileName,
                    RelativePath = relativePath,
                    IsVisibleToUser = isVisibleToUser,
                    IsDeleted = false,
                    CreatedOn = DateTime.Now,
                    CreatedBy = createdBy
                };

                await _db.ExternalFiles.AddAsync(entity);
                await _db.SaveChangesAsync();

                return entity.Id;
            }
            catch
            {
                return 0;
            }
        }

        public string ConvertPhysicalPathToRelativePath(string physicalPath, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(physicalPath) || string.IsNullOrWhiteSpace(webRootPath))
                return null;

            var fullPhysicalPath = Path.GetFullPath(physicalPath);
            var fullWebRootPath = Path.GetFullPath(webRootPath);

            if (!fullPhysicalPath.StartsWith(fullWebRootPath, StringComparison.OrdinalIgnoreCase))
                return null;

            var relative = fullPhysicalPath
                .Substring(fullWebRootPath.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return NormalizeRelativePath(relative);
        }

        public string BuildRelativePath(string rootFolderName, string pId, string maBenhAn, string storedFileName)
        {
            return NormalizeRelativePath(
                Path.Combine("uploads", rootFolderName, pId ?? "unknown", maBenhAn ?? "unknown", storedFileName ?? "")
            );
        }

        private string NormalizeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            return relativePath.Replace("\\", "/").TrimStart('/');
        }
    }
}