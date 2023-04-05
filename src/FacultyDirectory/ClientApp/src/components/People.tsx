import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ReactTable } from './ReactTable/ReactTable';
import { IPerson } from '../models/IPerson';
import { ISitePerson } from '../models/ISitePerson';
import { Cell, TableState, Column } from 'react-table';
import { SelectColumnFilter } from './ReactTable/Filtering';
import { Loading } from './Loading';

interface IPersonRecord {
  person: IPerson;
  sitePerson: ISitePerson;
}

export const People = () => {
  const [people, setPeople] = useState<IPersonRecord[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const getPeople = async () => {
      const results = await fetch('api/sitepeople').then(r => r.json());
      setPeople(results);
      setLoading(false);
    };

    getPeople();
  }, []);

  if (loading) {
    return <Loading text="LOADING..."></Loading>;
  }

  const navLink = ({ row }: Cell<IPersonRecord>) => {
    const { person } = row.original; // get the original data back for this row
    return <Link to={'/People/' + person.id}>View Record</Link>;
  };

  const decision = ({ sitePerson }: { sitePerson: ISitePerson }) => {
    // todo: decide what info we want to show for person sync status
    if (sitePerson) {
      return sitePerson.shouldSync ? 'sync' : 'exclude';
    } else {
      return 'pending';
    }
  };

  const columns: Column<IPersonRecord>[] = [
    { Header: '', id: 'detail', Cell: navLink },
    { Header: 'First', accessor: row => row.person.firstName },
    { Header: 'Last', id: 'lastName', accessor: row => row.person.lastName },
    {
      Header: 'Status',
      id: 'decision',
      accessor: decision,
      Filter: SelectColumnFilter,
      filter: 'includes'
    } as Column<IPersonRecord>
  ];

  // provide default column for sorting
  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: 'decision' }, { id: 'lastName' }]
  };

  return (
    <ReactTable columns={columns} data={people} initialState={initialState} />
  );
};
