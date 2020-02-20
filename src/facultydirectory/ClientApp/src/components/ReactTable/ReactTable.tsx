import React from 'react';
import {
  useTable,
  useFilters,
  useGlobalFilter,
  useSortBy,
  usePagination
} from 'react-table';
import { ColumnFilterHeaders, GlobalFilter } from './Filtering';
import { Pagination, PaginationItem, PaginationLink } from 'reactstrap';

export const ReactTable = ({ columns, data, initialState }: any) => {
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    prepareRow,
    state,
    flatColumns,
    preGlobalFilteredRows,
    setGlobalFilter,

    // pagination
    page,
    canPreviousPage,
    canNextPage,
    pageOptions,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,
    state: { pageIndex, pageSize }
  } = useTable(
    {
      columns,
      data,
      initialState: { ...initialState, pageIndex: 0, pageSize: 20 }
    },
    useFilters, // useFilters!
    useGlobalFilter, // useGlobalFilter!
    useSortBy,
    usePagination
  );

  return (
    <>
      <table className='table table-striped' {...getTableProps()}>
        <thead>
          <tr>
            <th
              colSpan={flatColumns.length}
              style={{
                textAlign: 'left'
              }}
            >
              <GlobalFilter
                preGlobalFilteredRows={preGlobalFilteredRows}
                globalFilter={state.globalFilter}
                setGlobalFilter={setGlobalFilter}
              />
            </th>
          </tr>
          {headerGroups.map(headerGroup => (
            <tr {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map(column => (
                <th {...column.getHeaderProps(column.getSortByToggleProps())}>
                  {column.render('Header')}
                  {/* Render the columns filter UI */}
                  <span>
                    {column.isSorted
                      ? column.isSortedDesc
                        ? ' 🔽'
                        : ' 🔼'
                      : ''}
                  </span>
                </th>
              ))}
            </tr>
          ))}
          <ColumnFilterHeaders headerGroups={headerGroups} />
        </thead>
        <tbody {...getTableBodyProps()}>
          {page.map((row, i) => {
            prepareRow(row);
            return (
              <tr {...row.getRowProps()}>
                {row.cells.map(cell => {
                  return (
                    <td {...cell.getCellProps()}>{cell.render('Cell')}</td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
      <Pagination className='pagination'>
        <PaginationItem onClick={() => gotoPage(0)} disabled={!canPreviousPage}>
          <PaginationLink first />
        </PaginationItem>
        <PaginationItem
          onClick={() => previousPage()}
          disabled={!canPreviousPage}
        >
          <PaginationLink previous />
        </PaginationItem>
        <PaginationItem>
          <PaginationLink>
            <span>
              Page{' '}
              <strong>
                {pageIndex + 1} of {pageOptions.length}
              </strong>{' '}
            </span>
            <span>
              | Go to page:{' '}
              <input
                type='number'
                defaultValue={pageIndex + 1}
                onChange={e => {
                  const page = e.target.value ? Number(e.target.value) - 1 : 0;
                  gotoPage(page);
                }}
                style={{ width: '100px' }}
              />
            </span>{' '}
            <select
              value={pageSize}
              onChange={e => {
                setPageSize(Number(e.target.value));
              }}
            >
              {[10, 20, 30, 40, 50].map(pageSize => (
                <option key={pageSize} value={pageSize}>
                  Show {pageSize}
                </option>
              ))}
            </select>{' '}
          </PaginationLink>
        </PaginationItem>
        <PaginationItem onClick={() => nextPage()} disabled={!canNextPage}>
          <PaginationLink next />
        </PaginationItem>
        <PaginationItem
          onClick={() => gotoPage(pageCount - 1)}
          disabled={!canNextPage}
        >
          <PaginationLink last />
        </PaginationItem>
      </Pagination>
    </>
  );
};
