using PTJ.Application.Common;
using PTJ.Application.DTOs.Company;
using PTJ.Application.DTOs.JobPost;

namespace PTJ.Application.Services;

public interface ISearchService
{
    Task<Result<PaginatedList<JobPostDto>>> SearchJobPostsAsync(SearchParameters parameters, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<CompanyDto>>> SearchCompaniesAsync(SearchParameters parameters, CancellationToken cancellationToken = default);
}
