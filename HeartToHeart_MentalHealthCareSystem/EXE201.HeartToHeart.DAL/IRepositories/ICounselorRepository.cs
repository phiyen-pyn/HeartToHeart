using EXE201.HeartToHeart.DAL.Entities.Application;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface ICounselorRepository
    {
        Task<Counselor?> GetCounselorByIdAsync(Guid counselorId);
        Task<Counselor?> GetCounselorByUserIdAsync(Guid userId);
        Task<IEnumerable<Counselor>> GetAvailableCounselorsAsync();
        Task<IEnumerable<Counselor>> GetCounselorsBySpecializationAsync(string specialization);
        Task<IEnumerable<Counselor>> GetAllCounselorsAsync();
        Task<bool> IsCounselorAvailableAsync(Guid counselorId);
        Task<bool> CreateCounselorAsync(Counselor counselor);
        Task<bool> UpdateCounselorAsync(Counselor counselor);
        Task<bool> DeleteCounselorAsync(Guid counselorId);
        Task<bool> CounselorExistsAsync(Guid counselorId);
        Task<bool> UserIsCounselorAsync(Guid userId);
        Task<int> GetTotalCounselorsCountAsync();
        Task<int> GetTotalCounselorsCountByMonthAsync(int year, int month);
        Task<int> GetVerifiedCounselorsCountAsync();
        Task<int> GetVerifiedCounselorsCountByMonthAsync(int year, int month);
        Task<int> GetAvailableCounselorsCountAsync();
        Task<IEnumerable<Appointment>> GetCounselorAppointmentsAsync(Guid counselorId, int page = 1, int pageSize = 10);
    }
}
