import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ReactTable, SelectColumnFilter } from './ReactTable';
import { IPerson } from '../models/IPerson';
import { ISitePerson } from '../models/ISitePerson';
import { Cell, TableState, Column } from 'react-table';

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
    return <span>loading...</span>;
  }

  // TOOD: start paging when record get large
  const orderedPeople = people.sort((a, b) =>
    a.person.lastName.localeCompare(b.person.lastName)
  );

  const navLink = ({ row }: Cell<IPersonRecord>) => {
    const { person } = row.original; // get the original data back for this row
    return <Link to={'/People/' + person.id}>View Record</Link>;
  };

  const decision = ({ sitePerson }: { sitePerson: ISitePerson }) => {
    // todo: decide what info we want to show for person sync status
    if (sitePerson) {
      return sitePerson.shouldSync ? 'sync' : 'hold';
    } else {
      return 'none';
    }
  };

  const columns: Column<IPersonRecord>[] = [
    { Header: '', id: 'detail', Cell: navLink },
    { Header: 'First', accessor: 'person.firstName' },
    { Header: 'Last', accessor: 'person.lastName' },
    {
      Header: 'Decision',
      id: 'decision',
      accessor: decision,
      Filter: SelectColumnFilter,
      filter: 'includes'
    }
  ];

  // provide default column for sorting
  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: 'decision' }]
  };

  return (
    <ReactTable
      columns={columns}
      data={orderedPeople}
      initialState={initialState}
    />
  );
};
