export interface BusinessLocationDto {
  readonly city: string;
  readonly street: string;
}

export interface BusinessDto {
  readonly businessId: string;
  readonly ownerId: string;
  readonly name: string;
  readonly photoUrl: string;
  readonly raiting: number;
  readonly description: string;
  readonly location: BusinessLocationDto;
}

export interface PagedBusinessesResponse {
  readonly items: readonly BusinessDto[];
  readonly totalCount: number;
  readonly page: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasNextPage: boolean;
}

export interface BusinessServiceItemDto {
  readonly bookableServiceId: string;
  readonly employeeId: string;
  readonly category: string;
  readonly serviceName: string;
  readonly price: number;
  readonly duration: string;
}

export interface BusinessEmployeeDto {
  readonly employeeId: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly position: string;
  readonly photoUrl: string;
}

export interface BusinessDetailsDto {
  readonly id: string;
  readonly ownerId: string;
  readonly name: string;
  readonly description: string;
  readonly location: BusinessLocationDto;
  readonly sevices: readonly BusinessServiceItemDto[];
  readonly employees: readonly BusinessEmployeeDto[];
  readonly status: string;
  readonly category: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
}
