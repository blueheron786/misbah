using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Application.Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IMapper _mapper;

        public FolderService(IFolderRepository folderRepository, IMapper mapper)
        {
            _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<FolderNodeDto> GetFolderByPathAsync(string path)
        {
            var folder = await _folderRepository.GetByPathAsync(path);
            return _mapper.Map<FolderNodeDto>(folder);
        }

        public async Task<IEnumerable<FolderNodeDto>> GetAllFoldersAsync()
        {
            var folders = await _folderRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<FolderNodeDto>>(folders);
        }

        public async Task<FolderNodeDto> CreateFolderAsync(FolderNodeDto folderDto)
        {
            var folder = _mapper.Map<FolderNode>(folderDto);
            var createdFolder = await _folderRepository.AddAsync(folder);
            return _mapper.Map<FolderNodeDto>(createdFolder);
        }

        public async Task UpdateFolderAsync(FolderNodeDto folderDto)
        {
            var existingFolder = await _folderRepository.GetByPathAsync(folderDto.Path);
            if (existingFolder == null)
            {
                throw new KeyNotFoundException($"Folder with path {folderDto.Path} not found.");
            }

            var folder = _mapper.Map<FolderNode>(folderDto);
            await _folderRepository.UpdateAsync(folder);
        }

        public async Task DeleteFolderAsync(string path)
        {
            await _folderRepository.DeleteAsync(path);
        }
    }
}
