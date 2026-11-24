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
