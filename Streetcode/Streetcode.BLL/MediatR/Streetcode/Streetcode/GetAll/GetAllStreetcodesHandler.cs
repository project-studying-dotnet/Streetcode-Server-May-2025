﻿using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAll;

public class GetAllStreetcodesHandler : IRequestHandler<GetAllStreetcodesQuery, Result<GetAllStreetcodesResponseDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetAllStreetcodesHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<GetAllStreetcodesResponseDTO>> Handle(GetAllStreetcodesQuery query, CancellationToken cancellationToken)
    {
        var filterRequest = query.request;

        var streetcodes = _repositoryWrapper.StreetcodeRepository
            .FindAll();

        if (filterRequest.Title is not null)
        {
            FindStreetcodesWithMatchTitle(ref streetcodes, filterRequest.Title);
        }

        if (filterRequest.Sort is not null)
        {
            FindSortedStreetcodes(ref streetcodes, filterRequest.Sort);
        }

        if (filterRequest.Filter is not null)
        {
            FindFilteredStreetcodes(ref streetcodes, filterRequest.Filter);
        }

        int pagesAmount = ApplyPagination(ref streetcodes, filterRequest.Amount, filterRequest.Page);

        var streetcodeDtos = _mapper.Map<IEnumerable<StreetcodeDTO>>(streetcodes.AsEnumerable());

        var response = new GetAllStreetcodesResponseDTO
        {
            Pages = pagesAmount,
            Streetcodes = streetcodeDtos
        };

        return Result.Ok(response);
    }

    private static void FindStreetcodesWithMatchTitle(
        ref IQueryable<StreetcodeContent> streetcodes,
        string title)
    {
        streetcodes = streetcodes.Where(s => s.Title
            .ToLower()
            .Contains(title
            .ToLower()) || s.Index
            .ToString() == title);
    }

    private static void FindFilteredStreetcodes(
        ref IQueryable<StreetcodeContent> streetcodes,
        string filter)
    {
        var filterParams = filter.Split(':');
        var filterValue = filterParams[1];

        streetcodes = streetcodes
            .AsEnumerable()
            .Where(s => filterValue.Contains(s.Status.ToString()))
            .AsQueryable();
    }

    private void FindSortedStreetcodes(
        ref IQueryable<StreetcodeContent> streetcodes,
        string sort)
    {
        var sortedRecords = streetcodes;

        var sortColumn = sort.Trim();
        var sortDirection = "asc";

        if (sortColumn.StartsWith("-"))
        {
            sortDirection = "desc";
            sortColumn = sortColumn.Substring(1);
        }

        var type = typeof(StreetcodeContent);
        var parameter = Expression.Parameter(type, "p");
        var property = Expression.Property(parameter, sortColumn);
        var lambda = Expression.Lambda(property, parameter);

        streetcodes = sortDirection switch
        {
            "asc" => Queryable.OrderBy(sortedRecords, (dynamic)lambda),
            "desc" => Queryable.OrderByDescending(sortedRecords, (dynamic)lambda),
            _ => sortedRecords,
        };
    }

    private static int ApplyPagination(
        ref IQueryable<StreetcodeContent> streetcodes,
        int amount,
        int page)
    {
        var totalPages = (int)Math.Ceiling(streetcodes.Count() / (double)amount);

        streetcodes = streetcodes
            .Skip((page - 1) * amount)
            .Take(amount);

        return totalPages;
    }
}