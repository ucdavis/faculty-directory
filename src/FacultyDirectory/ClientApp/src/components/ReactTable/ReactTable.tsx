
import {
  useTable,
  useFilters,
  useGlobalFilter,
  useSortBy,
  usePagination
} from 'react-table';
import { ColumnFilterHeaders, GlobalFilter } from './Filtering';

export const ReactTable = ({ columns, data, initialState }: any) => {
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    prepareRow,
    state,
    allColumns,
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
      <table
        className='table table-bordered table-striped'
        {...getTableProps()}
      >
        <thead>
          <tr>
            <th
              colSpan={allColumns.length}
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
          {headerGroups.map(headerGroup => {
            const { key, ...headerGroupProps } = headerGroup.getHeaderGroupProps();
            return (
            <tr key={key} {...headerGroupProps}>
              {headerGroup.headers.map(column => {
                const { key: colKey, ...columnProps } = column.getHeaderProps(column.getSortByToggleProps());
                return (
                <th key={colKey} {...columnProps}>
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
                );
              })}
            </tr>
            );
          })}
          <ColumnFilterHeaders headerGroups={headerGroups} />
        </thead>
        <tbody {...getTableBodyProps()}>
          {page.map((row, _i) => {
            prepareRow(row);
            const { key: rowKey, ...rowProps } = row.getRowProps();
            return (
              <tr key={rowKey} {...rowProps}>
                {row.cells.map(cell => {
                  const { key: cellKey, ...cellProps } = cell.getCellProps();
                  return (
                    <td key={cellKey} {...cellProps}>{cell.render('Cell')}</td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
      <nav>
        <ul className='pagination'>
          <li className={`page-item ${!canPreviousPage ? 'disabled' : ''}`}>
            <a className='page-link' href='#' onClick={(e) => { e.preventDefault(); gotoPage(0); }}>First</a>
          </li>
          <li className={`page-item ${!canPreviousPage ? 'disabled' : ''}`}>
            <a className='page-link' href='#' onClick={(e) => { e.preventDefault(); previousPage(); }}>Previous</a>
          </li>
          <li className='page-item'>
            <span className='page-link'>
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
            </span>
          </li>
          <li className={`page-item ${!canNextPage ? 'disabled' : ''}`}>
            <a className='page-link' href='#' onClick={(e) => { e.preventDefault(); nextPage(); }}>Next</a>
          </li>
          <li className={`page-item ${!canNextPage ? 'disabled' : ''}`}>
            <a className='page-link' href='#' onClick={(e) => { e.preventDefault(); gotoPage(pageCount - 1); }}>Last</a>
          </li>
        </ul>
      </nav>
    </>
  );
};
