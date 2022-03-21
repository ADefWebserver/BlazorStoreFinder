#nullable disable
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace BlazorStoreFinder
{
    public class StoreLocationService
    {
        private readonly BlazorStoreFinderContext _context;
        public StoreLocationService(BlazorStoreFinderContext context)
        {
            _context = context;
        }
        public async Task<List<StoreLocations>> GetStoreLocations()
        {
            return await _context.StoreLocations.OrderBy(x => x.LocationName).ToListAsync();
        }

        public async Task<StoreLocations> GetStoreLocation(int id)
        {
            return await _context.StoreLocations.FindAsync(id);
        }
        public async Task<StoreLocations> AddStoreLocation(StoreLocations storeLocation)
        {
            _context.StoreLocations.Add(storeLocation);
            await _context.SaveChangesAsync();
            return storeLocation;
        }
        public async Task<StoreLocations> UpdateStoreLocation(StoreLocations storeLocation)
        {
            _context.StoreLocations.Update(storeLocation);
            await _context.SaveChangesAsync();
            return storeLocation;
        }
        public async Task<StoreLocations> DeleteStoreLocation(int id)
        {
            var storeLocation = await _context.StoreLocations.FindAsync(id);
            _context.StoreLocations.Remove(storeLocation);
            await _context.SaveChangesAsync();
            return storeLocation;
        }     

    }
}