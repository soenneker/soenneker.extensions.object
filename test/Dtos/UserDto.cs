﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Soenneker.Dtos.StreetAddress;

namespace Soenneker.Extensions.Object.Tests.Dtos;

public class UserDto
{
    public int UserId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    public string LastName { get; set; } = default!;

    public DateTime BirthDate { get; set; }

    public string Email { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public bool IsActive { get; set; }

    public decimal Salary { get; set; }

    public DateTime LastLogin { get; set; }

    public List<PermissionDto>? Permissions { get; set; }

    public StreetAddress Address { get; set; }
}