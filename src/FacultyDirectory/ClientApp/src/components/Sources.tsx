import React from 'react';
import { ISource } from '../models/ISource';

interface IProps {
  sources: ISource[];
  onChange: () => any;
}

// TODO: find a better way to pull static list of all source types
const sourceTypes = ['scholar'];

export const Sources = (props: IProps) => {
  const renderSources = () => {
    const sources = sourceTypes.map(sourceType => {
        const sourceForType = props.sources.find(s => s.source === sourceType);

        return (
          <span className='sources' key={sourceType}>
            {sourceType} - {sourceForType === undefined ? 'pending' : sourceForType?.sourceKey || 'not found'}
          </span>
        );
    });

    return sources;
  };

  return (
    <div>
      <p className='sourceIDs'>{renderSources()}</p>
    </div>
  );
};
