import React from 'react';
import { useTable, useFilters, useGlobalFilter, useSortBy } from 'react-table';
import { ColumnFilterHeaders, GlobalFilter } from './Filtering';

export const ReactTable = ({ columns, data, initialState }: any) => {
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    rows,
    prepareRow,
    state,
    flatColumns,
    preGlobalFilteredRows,
    setGlobalFilter
  } = useTable(
    {
      columns,
      data,
      initialState
    },
    useFilters, // useFilters!
    useGlobalFilter, // useGlobalFilter!
    useSortBy
  );

  return (
    <table className='table table-striped table-dark' {...getTableProps()}>
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
                  {column.isSorted ? (column.isSortedDesc ? ' 🔽' : ' 🔼') : ''}
                </span>
              </th>
            ))}
          </tr>
        ))}
        {ColumnFilterHeaders(headerGroups)}
      </thead>
      <tbody {...getTableBodyProps()}>
        {rows.map((row, i) => {
          prepareRow(row);
          return (
            <tr {...row.getRowProps()}>
              {row.cells.map(cell => {
                return <td {...cell.getCellProps()}>{cell.render('Cell')}</td>;
              })}
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};
